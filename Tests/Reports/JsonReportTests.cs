using System.Text.Json;

using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Reports;

namespace Tests.Reports;

public class JsonReportTests
{
    [Fact]
    public void TestJsonReportGeneration()
    {
        FileChecksum[] checksums =
        [
            new("xyz", FileType.Binary, "abc"),
            new("xyz", FileType.Other, "def"),
        ];

        JsonReport report = new JsonReport();
        string jsonText = report.CreateReport(checksums);

        Assert.Equal(JsonSerializer.Serialize(checksums, new JsonSerializerOptions()
        {
            WriteIndented = true
        }), jsonText);
    }
}