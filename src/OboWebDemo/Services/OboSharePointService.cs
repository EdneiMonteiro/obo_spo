// Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
// See LICENSE and DISCLAIMER.md in the project root for details.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OboWebDemo.Models;

namespace OboWebDemo.Services;

public sealed class OboSharePointOptions
{
    public string SharePointUrl { get; init; } = string.Empty;
    public string[] GraphScopes { get; init; } = [];
    public int Top { get; init; } = 10;
}

public sealed class OboSharePointService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OboSharePointOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public OboSharePointService(
        ITokenAcquisition tokenAcquisition,
        IHttpClientFactory httpClientFactory,
        IOptions<OboSharePointOptions> options)
    {
        _tokenAcquisition = tokenAcquisition;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<OboResult> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var result = new OboResult();

        result.AddStep("User authenticated via OpenID Connect");

        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(_options.GraphScopes);
        result.AddStep($"OBO token acquired for Graph scopes: {string.Join(", ", _options.GraphScopes)}");

        using var httpClient = _httpClientFactory.CreateClient("Graph");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var target = await ResolveSharePointTargetAsync(httpClient, _options.SharePointUrl, result, cancellationToken);

        result.SiteName = target.Site.DisplayName ?? target.Site.Name ?? target.Site.Id;
        result.LibraryName = target.Drive.Name ?? target.Drive.Id;

        var requestUri = BuildDriveChildrenRequestUri(target.Drive.Id, target.FolderPathEncoded, _options.Top);
        result.AddStep($"Listing items: GET {requestUri}");

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Graph request failed with {(int)response.StatusCode} {response.ReasonPhrase}.\n{content}");
        }

        var graphResponse = JsonSerializer.Deserialize<GraphDriveItemListResponse>(content, JsonOptions);
        result.Items = (graphResponse?.Value ?? [])
            .OrderByDescending(item => item.LastModifiedDateTime ?? DateTimeOffset.MinValue)
            .ToList();

        result.AddStep($"Graph returned {result.Items.Count} item(s)");
        return result;
    }

    private async Task<SharePointTarget> ResolveSharePointTargetAsync(
        HttpClient httpClient, string sharePointUrl, OboResult result, CancellationToken cancellationToken)
    {
        var sharePointUri = new Uri(sharePointUrl, UriKind.Absolute);
        var originalPath = NormalizePath(sharePointUri.AbsolutePath);

        result.AddStep($"Resolving SharePoint URL: {sharePointUrl}");

        foreach (var candidateSitePath in GetCandidateSitePaths(originalPath))
        {
            var site = await TryGetSiteAsync(httpClient, sharePointUri.Host, candidateSitePath, cancellationToken);
            if (site is null) continue;

            result.AddStep($"Resolved site: {site.DisplayName ?? site.Name} (path: {candidateSitePath})");

            var remainderPath = GetRemainderPath(originalPath, candidateSitePath);
            var decodedSegments = DecodePathSegments(remainderPath);
            GraphDrive drive;
            var folderSegments = Array.Empty<string>();

            if (decodedSegments.Length > 0)
            {
                drive = await GetDriveByNameAsync(httpClient, site.Id, decodedSegments[0], cancellationToken);
                folderSegments = decodedSegments.Skip(1).ToArray();
                result.AddStep($"Resolved document library: {drive.Name}");
            }
            else
            {
                drive = await GetDefaultDriveAsync(httpClient, site.Id, cancellationToken);
                result.AddStep($"Using default document library: {drive.Name}");
            }

            var folderPathDisplay = folderSegments.Length == 0 ? null : string.Join('/', folderSegments);
            var folderPathEncoded = folderSegments.Length == 0 ? null : EncodePathSegments(folderSegments);

            if (folderPathDisplay is not null)
            {
                result.AddStep($"Resolved folder: /{folderPathDisplay}");
                result.FolderPath = folderPathDisplay;
            }

            return new SharePointTarget
            {
                Site = site,
                Drive = drive,
                FolderPathEncoded = folderPathEncoded
            };
        }

        throw new InvalidOperationException(
            $"Could not resolve a SharePoint site or document library from '{sharePointUrl}'.");
    }

    private async Task<GraphSite?> TryGetSiteAsync(HttpClient httpClient, string host, string candidateSitePath, CancellationToken cancellationToken)
    {
        var requestUri = candidateSitePath == "/"
            ? $"sites/{host}?$select=id,displayName,name,webUrl"
            : $"sites/{host}:{candidateSitePath}?$select=id,displayName,name,webUrl";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode) throw CreateGraphException(requestUri, response, content);

        return JsonSerializer.Deserialize<GraphSite>(content, JsonOptions);
    }

    private async Task<GraphDrive> GetDefaultDriveAsync(HttpClient httpClient, string siteId, CancellationToken cancellationToken)
    {
        var requestUri = $"sites/{Uri.EscapeDataString(siteId)}/drive?$select=id,name,webUrl,driveType";
        return await GetGraphAsync<GraphDrive>(httpClient, requestUri, cancellationToken);
    }

    private async Task<GraphDrive> GetDriveByNameAsync(HttpClient httpClient, string siteId, string driveName, CancellationToken cancellationToken)
    {
        var requestUri = $"sites/{Uri.EscapeDataString(siteId)}/drives?$select=id,name,webUrl,driveType";
        var response = await GetGraphAsync<GraphDriveListResponse>(httpClient, requestUri, cancellationToken);
        var drive = response.Value.FirstOrDefault(
            item => string.Equals(item.Name, driveName, StringComparison.OrdinalIgnoreCase));

        if (drive is not null) return drive;

        var available = response.Value.Select(item => item.Name).Where(n => !string.IsNullOrWhiteSpace(n));
        throw new InvalidOperationException(
            $"Could not find document library '{driveName}'. Available: {string.Join(", ", available)}");
    }

    private async Task<T> GetGraphAsync<T>(HttpClient httpClient, string requestUri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) throw CreateGraphException(requestUri, response, content);
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
               ?? throw new InvalidOperationException($"Graph returned empty payload for '{requestUri}'.");
    }

    private static Exception CreateGraphException(string requestUri, HttpResponseMessage response, string content)
    {
        return new InvalidOperationException(
            $"Graph '{requestUri}' failed with {(int)response.StatusCode} {response.ReasonPhrase}.\n{content}");
    }

    private static string BuildDriveChildrenRequestUri(string driveId, string? folderPathEncoded, int top)
    {
        const string select = "$select=id,name,webUrl,lastModifiedDateTime,size,file,folder";
        var escaped = Uri.EscapeDataString(driveId);
        return string.IsNullOrWhiteSpace(folderPathEncoded)
            ? $"drives/{escaped}/root/children?$top={top}&{select}"
            : $"drives/{escaped}/root:/{folderPathEncoded}:/children?$top={top}&{select}";
    }

    private static IReadOnlyList<string> GetCandidateSitePaths(string originalPath)
    {
        if (originalPath == "/") return ["/"];
        var segments = originalPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var candidates = new List<string>(segments.Length + 1);
        for (var length = segments.Length; length >= 1; length--)
            candidates.Add($"/{string.Join('/', segments.Take(length))}");
        candidates.Add("/");
        return candidates;
    }

    private static string NormalizePath(string absolutePath)
    {
        var trimmed = absolutePath.TrimEnd('/');
        return string.IsNullOrWhiteSpace(trimmed) ? "/" : trimmed;
    }

    private static string GetRemainderPath(string originalPath, string candidateSitePath)
    {
        if (candidateSitePath == "/") return originalPath.Trim('/');
        if (string.Equals(originalPath, candidateSitePath, StringComparison.OrdinalIgnoreCase)) return string.Empty;
        return originalPath[(candidateSitePath.Length + 1)..];
    }

    private static string[] DecodePathSegments(string remainderPath)
    {
        return remainderPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString).ToArray();
    }

    private static string EncodePathSegments(IEnumerable<string> pathSegments)
    {
        return string.Join('/', pathSegments.Select(Uri.EscapeDataString));
    }

    private sealed class SharePointTarget
    {
        public required GraphSite Site { get; init; }
        public required GraphDrive Drive { get; init; }
        public string? FolderPathEncoded { get; init; }
    }
}
