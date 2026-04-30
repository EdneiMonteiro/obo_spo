/*
This Sample Code is provided for the purpose of illustration only and is not
intended to be used in a production environment. THIS SAMPLE CODE AND ANY
RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

We grant You a nonexclusive, royalty-free right to use and modify the Sample
Code and to reproduce and distribute the object code form of the Sample Code,
provided that You agree: (i) to not use Our name, logo, or trademarks to market
Your software product in which the Sample Code is embedded; (ii) to include a
valid copyright notice on Your software product in which the Sample Code is
embedded; and (iii) to indemnify, hold harmless, and defend Us and Our
suppliers from and against any claims or lawsuits, including attorneys' fees,
that arise or result from the use or distribution of the Sample Code.

Please note: None of the conditions outlined in the disclaimer above will
supersede the terms and conditions contained within the Customers Support
Services Description.
*/

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
