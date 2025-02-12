using OOP_DesignPatterns_Project3.Algorithms;
using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Utils;

namespace OOP_DesignPatterns_Project3;

public sealed class CalculationLogic : IOperationLogic
{
    private readonly DirectoryInfo m_path;
    private readonly IChecksumAlgorithm m_algorithm;
    private bool m_shouldExit = false;

    public CalculationLogic(DirectoryInfo path, Algorithms.Algorithms algorithm)
    {
        m_path = path;
        m_algorithm = algorithm switch
        {
            Algorithms.Algorithms.MD5 => new MD5Algorithm(nameof(CalculationLogic)),
            Algorithms.Algorithms.SHA1 => new MD5Algorithm(nameof(CalculationLogic)), //TODO: Change
            Algorithms.Algorithms.SHA2 => new MD5Algorithm(nameof(CalculationLogic)), //TODO: Change
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT,
            new EventListener($"{EventMaster.EVENT_ID_EXIT}.calculationLogic", ExitListener));
    }

    private void ExitListener(IEvent @event) => m_shouldExit = true;

    public void Start()
    {
        List<FileChecksum> checksums = [];
        checksums.AddRange(PerformChecksumOnFiles(m_path));

        foreach (var dir in m_path.GetDirectories())
        {
            if (m_shouldExit)
                break;
            
            checksums.AddRange(PerformChecksumOnFiles(dir));
        }

        EventMaster.Invoke(EventMaster.EVENT_ID_EXIT_CONFIRM, new EmptyEvent());
    }

    private List<FileChecksum> PerformChecksumOnFiles(DirectoryInfo di)
    {
        List<FileChecksum> checksums = [];
        FileInfo[] files;

        try
        {
            files = di.GetFiles();
        }
        catch (Exception e)
        {
            return checksums;
        }

        foreach (var file in files)
        {
            ConsoleInput.CheckForInput();
            
            Console.WriteLine($"Processing: {file.FullName}");

            FileType type = IsBinaryFile(file) ? FileType.Binary : FileType.Other;
            string checksum = m_algorithm.PerformAlgorithm(file.FullName);

            if (m_shouldExit)
                break;

            if (checksum.Length == 0) //skip if there was an error creating the hash.
            {
                Console.WriteLine("\t\tError!\n");

                continue;
            }

            Console.WriteLine("\t\tOK!\n");

            checksums.Add(new FileChecksum(file.FullName, type, checksum));

            ConsoleInput.CheckForInput();
        }

        return checksums;
    }

    private static bool IsBinaryFile(FileInfo file)
    {
        try
        {
            byte[] buffer = new byte[256];
            using (FileStream stream = file.OpenRead())
            {
                stream.ReadExactly(buffer, 0, (int) Math.Min(buffer.LongLength, stream.Length));
            }

            foreach (byte t in buffer)
            {
                if (t != 0 && t is < 32 or > 126 && t != 9 && t != 10 && t != 13)
                    return true;
            }

            return false;
        }
        catch (Exception e)
        {
            return true;
        }
    }
}