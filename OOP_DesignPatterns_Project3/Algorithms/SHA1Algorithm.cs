using System.Security.Cryptography;

using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Utils;

namespace OOP_DesignPatterns_Project3.Algorithms;

public class SHA1Algorithm : IChecksumAlgorithm
{
    private readonly string m_callerID;
    private bool m_shouldExit = false;

    public SHA1Algorithm(string callerId)
    {
        m_callerID = callerId;

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT,
            new EventListener($"{EventMaster.EVENT_ID_EXIT}.md5Alg", ExitListener));
    }

    private void ExitListener(IEvent @event) => m_shouldExit = true;

    public string PerformAlgorithm(string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;

        Stream stream;
        try
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            return string.Empty;
        }

        using var md5 = SHA1.Create();

        byte[] buffer = new byte[8192];
        int bytesRead;
        long totalBytesRead = 0;
        long fileLength = stream.Length;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (m_shouldExit)
                return "0";
            
            ConsoleInput.CheckForInput();
            
            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            totalBytesRead += bytesRead;

            int progress = (int) ((totalBytesRead * 100) / fileLength);

            EventMaster.Invoke(EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE,
                new FileProgressUpdateEvent(m_callerID, progress));
            
            ConsoleInput.CheckForInput();
        }

        md5.TransformFinalBlock(buffer, 0, 0);
        stream.Close();

        return BitConverter.ToString(md5.Hash ?? []).Replace("-", "").ToLowerInvariant();
    }
}