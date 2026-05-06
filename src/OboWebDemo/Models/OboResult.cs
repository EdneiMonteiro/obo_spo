// Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
// See LICENSE and DISCLAIMER.md in the project root for details.

namespace OboWebDemo.Models;

public sealed class OboResult
{
    public string? SiteName { get; set; }
    public string? LibraryName { get; set; }
    public string? FolderPath { get; set; }
    public List<GraphDriveItem> Items { get; set; } = [];
    public List<OboStep> Steps { get; } = [];

    public void AddStep(string description)
    {
        Steps.Add(new OboStep
        {
            Order = Steps.Count + 1,
            Timestamp = DateTimeOffset.UtcNow,
            Description = description
        });
    }
}

public sealed class OboStep
{
    public int Order { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string Description { get; init; } = string.Empty;
}
