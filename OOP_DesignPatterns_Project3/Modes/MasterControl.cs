using OOP_DesignPatterns_Project3.Events;

namespace OOP_DesignPatterns_Project3;

public sealed class MasterControl
{
    private readonly IOperationLogic m_operationLogic;

    public MasterControl(DirectoryInfo path, Algorithms.Algorithms algorithm, FileInfo checksums)
    {
        m_operationLogic = algorithm == Algorithms.Algorithms.None
            ? new VerificationLogic(path, checksums)
            : new CalculationLogic(path, algorithm);
        
        EventMaster.Bind("pause", new EventListener("masterControl.pause", PauseListener));
    }

    public void Start() => new Thread(() => m_operationLogic.Start()).Start();

    private void PauseListener(IEvent @event)
    {
        Console.WriteLine("[MasterControl] Pause received!");
    }
}