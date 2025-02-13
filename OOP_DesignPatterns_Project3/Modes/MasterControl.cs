using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Reports;

namespace OOP_DesignPatterns_Project3.Modes;

public sealed class MasterControl
{
    private readonly IOperationLogic m_operationLogic;

    public MasterControl(FileSystemInfo path, Algorithms.Algorithms algorithm, FileInfo checksums, ReportTypes format)
    {
        m_operationLogic = algorithm == Algorithms.Algorithms.None
            ? new VerificationLogic(path, checksums)
            : new CalculationLogic(path, algorithm, format);
    }

    public void Start() => new Thread(() => m_operationLogic.Start()).Start();
}