using System.Security.Cryptography;

using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Utils;

namespace OOP_DesignPatterns_Project3.Algorithms;

public class MD5Algorithm : IChecksumAlgorithm
{
    private readonly string m_callerID;
    private bool m_shouldExit = false;

    private bool m_shouldWaitForKeypress = false;

    public MD5Algorithm(string callerId)
    {
        m_callerID = callerId;

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT,
            new EventListener($"{EventMaster.EVENT_ID_EXIT}.md5Alg", ExitListener));
    }

    private void ExitListener(IEvent @event) => m_shouldExit = true;

    public void SetWaitForKeypress(bool value) => m_shouldWaitForKeypress = value;

    public string PerformAlgorithm(Stream fileStream)
    {
        using var md5 = MD5.Create();

        byte[] buffer = new byte[8192];
        int bytesRead;
        long totalBytesRead = 0;
        long fileLength = fileStream.Length;

        while ((bytesRead = fileStream.Read(buffer, 0, (int) Math.Min(buffer.LongLength, fileLength))) > 0)
        {
            if (m_shouldExit)
                return "0";

            if(m_shouldWaitForKeypress)
                ConsoleInput.CheckForInput();

            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            totalBytesRead += bytesRead;

            int progress = (int) ((totalBytesRead * 100) / fileLength);

            EventMaster.Invoke(EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE,
                new FileProgressUpdateEvent(m_callerID, progress));

            if(m_shouldWaitForKeypress)
                ConsoleInput.CheckForInput();
        }

        md5.TransformFinalBlock(buffer, 0, 0);

        return BitConverter.ToString(md5.Hash ?? []).Replace("-", "").ToLowerInvariant();
    }
}