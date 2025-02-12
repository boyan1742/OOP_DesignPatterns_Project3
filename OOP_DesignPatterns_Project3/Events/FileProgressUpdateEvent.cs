namespace OOP_DesignPatterns_Project3.Events;

public class FileProgressUpdateEvent : IEvent
{
    private readonly string m_callerID;
    private readonly int m_progress;
    
    public FileProgressUpdateEvent(string callerID, int progress)
    {
        m_callerID = callerID;
        m_progress = progress;
    }

    public int GetProgress() => m_progress;
    
    public string GetCallerID() => m_callerID;
}