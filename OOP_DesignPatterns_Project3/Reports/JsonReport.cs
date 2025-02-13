using System.Text.Json;

using OOP_DesignPatterns_Project3.Data;

namespace OOP_DesignPatterns_Project3.Reports;

public class JsonReport : IReport
{
    private static JsonSerializerOptions Options { get; } = new()
    {
        WriteIndented = true
    };

    public string CreateReport(FileChecksum[] checksums) => JsonSerializer.Serialize(checksums, Options);
}