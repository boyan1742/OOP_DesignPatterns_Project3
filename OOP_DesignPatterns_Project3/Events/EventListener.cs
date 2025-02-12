namespace OOP_DesignPatterns_Project3.Events;

public sealed class EventListener
{
    public string EventListenerID { get; }
    public Action<IEvent> EventListenerAction { get; }

    public EventListener(string eventListenerId, Action<IEvent> eventListenerAction)
    {
        if (string.IsNullOrEmpty(eventListenerId))
            throw new ArgumentNullException(nameof(eventListenerId));

        EventListenerID = eventListenerId;
        EventListenerAction = eventListenerAction;
    }
}