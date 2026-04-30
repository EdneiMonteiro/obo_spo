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
