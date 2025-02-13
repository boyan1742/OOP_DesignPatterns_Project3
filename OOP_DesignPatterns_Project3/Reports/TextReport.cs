using System.Text;

using OOP_DesignPatterns_Project3.Data;

namespace OOP_DesignPatterns_Project3.Reports;

public class TextReport : IReport
{
    public string CreateReport(FileChecksum[] checksums)
    {
        var sb = new StringBuilder();

        foreach (var checksum in checksums)
        {
            sb.Append(checksum.Checksum).Append(' ')
                .Append(checksum.Type == FileType.Binary ? '*' : ' ')
                .Append(checksum.Path)
                .Append('\n');
        }

        return sb.ToString().TrimEnd('\n');
    }
}