using OOP_DesignPatterns_Project3.Data;

namespace OOP_DesignPatterns_Project3.Reports;

public interface IReport
{
    string CreateReport(FileChecksum[] checksums);
}