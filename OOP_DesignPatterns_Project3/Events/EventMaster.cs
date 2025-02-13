using System.Runtime.InteropServices;

namespace OOP_DesignPatterns_Project3.Events;

public static class EventMaster
{
    private static Dictionary<string, List<EventListener>> m_eventHandlers = [];

    public const string EVENT_ID_PAUSE = "pause";
    public const string EVENT_ID_EXIT = "exit";
    public const string EVENT_ID_FILE_PROGRESS_UPDATE = "progressUpdate";
    public const string EVENT_ID_EXIT_CONFIRM = "exitConfirm";

    public static bool Bind(string eventID, EventListener listener)
    {
        if (string.IsNullOrEmpty(eventID))
            return false;

        lock (m_eventHandlers)
        {
            ref List<EventListener>? listeners =
                ref CollectionsMarshal.GetValueRefOrAddDefault(m_eventHandlers, eventID, out bool exists);

            listeners ??= [];

            if (exists)
            {
                if (listeners.All(x => x.EventListenerID != listener.EventListenerID))
                    listeners.Add(listener);
                else
                    return false;

                return true;
            }

            listeners.Add(listener);
        }

        return true;
    }

    public static void Invoke(string eventID, IEvent @event)
    {
        if (string.IsNullOrEmpty(eventID))
            return;

        if (!m_eventHandlers.TryGetValue(eventID, out List<EventListener>? list))
            return;

        foreach (EventListener eventListener in list)
            eventListener.EventListenerAction.Invoke(@event);
    }

    public static async Task InvokeAsync(string eventID, IEvent @event)
    {
        if (string.IsNullOrEmpty(eventID))
            return;

        if (!m_eventHandlers.TryGetValue(eventID, out List<EventListener>? list))
            return;

        foreach (EventListener eventListener in list)
            await Task.Run(() => eventListener.EventListenerAction.Invoke(@event));
    }
}