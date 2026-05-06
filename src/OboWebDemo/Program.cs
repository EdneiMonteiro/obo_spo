// Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
// See LICENSE and DISCLAIMER.md in the project root for details.

using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using OboWebDemo.Services;

LoadDotEnvIntoProcess();

// Map well-known env vars to ASP.NET configuration keys before building configuration.
MapEnv("AZURE_TENANT_ID", "AzureAd__TenantId");
MapEnv("AZURE_CLIENT_ID", "AzureAd__ClientId");
MapEnv("AZURE_CERTIFICATE_THUMBPRINT", "AzureAd__ClientCertificates__0__CertificateThumbprint");
MapEnv("SHAREPOINT_URL", "OboDemo__SharePointUrl");

var builder = WebApplication.CreateBuilder(args);

var missingConfiguration = GetMissingConfiguration(builder.Configuration);
if (missingConfiguration.Count > 0)
{
    throw new InvalidOperationException(
        "Missing required configuration for OboWebDemo. " +
        "Copy .env.example to .env in the repository root or set these environment variables: " +
        string.Join(", ", missingConfiguration));
}

static void MapEnv(string source, string target)
{
    var value = Environment.GetEnvironmentVariable(source);
    if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(target)))
        Environment.SetEnvironmentVariable(target, value);
}

static void LoadDotEnvIntoProcess()
{
    var envFile = FindFileInParents(Directory.GetCurrentDirectory(), ".env")
                  ?? FindFileInParents(AppContext.BaseDirectory, ".env");

    if (envFile is null)
        return;

    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            continue;

        var idx = trimmed.IndexOf('=');
        if (idx <= 0)
            continue;

        var key = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim();
        Environment.SetEnvironmentVariable(key, value);
    }
}

static string? FindFileInParents(string startPath, string fileName)
{
    var current = new DirectoryInfo(startPath);
    while (current is not null)
    {
        var candidate = Path.Combine(current.FullName, fileName);
        if (File.Exists(candidate))
            return candidate;

        current = current.Parent;
    }

    return null;
}

static IReadOnlyList<string> GetMissingConfiguration(ConfigurationManager configuration)
{
    var missing = new List<string>();

    AddIfMissing(missing, "AZURE_TENANT_ID", configuration["AzureAd:TenantId"]);
    AddIfMissing(missing, "AZURE_CLIENT_ID", configuration["AzureAd:ClientId"]);
    AddIfMissing(
        missing,
        "AZURE_CERTIFICATE_THUMBPRINT",
        configuration["AzureAd:ClientCertificates:0:CertificateThumbprint"]);
    AddIfMissing(missing, "SHAREPOINT_URL", configuration["OboDemo:SharePointUrl"]);

    return missing;
}

static void AddIfMissing(ICollection<string> missing, string key, string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        missing.Add(key);
}

builder.Services
    .AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi(
        builder.Configuration.GetSection("OboDemo:GraphScopes").Get<string[]>() ?? Array.Empty<string>())
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddHttpClient("Graph", client =>
{
    client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
});

builder.Services.Configure<OboSharePointOptions>(builder.Configuration.GetSection("OboDemo"));
builder.Services.AddScoped<OboSharePointService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
