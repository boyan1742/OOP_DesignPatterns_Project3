namespace OOP_DesignPatterns_Project3.Events;

public sealed class EmptyEvent : IEvent
{
    public string GetCallerID() => "";
}