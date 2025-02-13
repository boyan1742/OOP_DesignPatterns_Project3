using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Reports;

namespace Tests.Reports;

public class TextReportTests
{
    [Fact]
    public void TestTextReportGeneration()
    {
        FileChecksum[] checksums =
        [
            new("xyz", FileType.Binary, "abc"),
            new("xyz", FileType.Other, "def"),
        ];

        TextReport report = new TextReport();
        string text = report.CreateReport(checksums);

        Assert.Equal("abc *xyz\ndef  xyz", text);
    }
}