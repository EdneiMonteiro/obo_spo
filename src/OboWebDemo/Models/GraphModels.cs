// Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
// See LICENSE and DISCLAIMER.md in the project root for details.

namespace OboWebDemo.Models;

public sealed class GraphSite
{
    public string Id { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Name { get; init; }
    public string? WebUrl { get; init; }
}

public sealed class GraphDriveListResponse
{
    public List<GraphDrive> Value { get; init; } = [];
}

public sealed class GraphDrive
{
    public string Id { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? WebUrl { get; init; }
    public string? DriveType { get; init; }
}

public sealed class GraphDriveItemListResponse
{
    public List<GraphDriveItem> Value { get; init; } = [];
}

public sealed class GraphDriveItem
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? WebUrl { get; init; }
    public DateTimeOffset? LastModifiedDateTime { get; init; }
    public long? Size { get; init; }
    public GraphFileFacet? File { get; init; }
    public GraphFolderFacet? Folder { get; init; }
}

public sealed class GraphFileFacet { }
public sealed class GraphFolderFacet { }
