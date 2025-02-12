using OOP_DesignPatterns_Project3.Events;

namespace OOP_DesignPatterns_Project3.Utils;

public static class ConsoleInput
{
    public static void CheckForInput()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                //exit
                case ConsoleKey.Q:
                    EventMaster.Invoke(EventMaster.EVENT_ID_EXIT, new EmptyEvent());

                    break;
                //pause
                case ConsoleKey.P:
                    EventMaster.Invoke(EventMaster.EVENT_ID_PAUSE, new EmptyEvent());

                    break;
            }
        }
    }
}