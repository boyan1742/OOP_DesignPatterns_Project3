using System.Text;

using Lnk;

using OOP_DesignPatterns_Project3.Algorithms;
using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Reports;
using OOP_DesignPatterns_Project3.Utils;

namespace OOP_DesignPatterns_Project3.Modes;

public sealed class CalculationLogic : IOperationLogic
{
    private readonly FileSystemInfo m_path;
    private readonly Algorithms.Algorithms m_usedAlgorithm;
    private readonly IChecksumAlgorithm m_algorithm;
    private readonly ReportTypes m_format;
    private readonly bool m_shouldOutput;
    private bool m_shouldExit = false;

    private readonly List<string> m_visited = [];

    public CalculationLogic(FileSystemInfo path, Algorithms.Algorithms algorithm,
        ReportTypes format, bool shouldOutput = true)
    {
        m_path = path;
        m_usedAlgorithm = algorithm;
        m_format = format;
        m_shouldOutput = shouldOutput;
        m_algorithm = algorithm switch
        {
            Algorithms.Algorithms.MD5 => new MD5Algorithm(nameof(CalculationLogic)),
            Algorithms.Algorithms.SHA1 => new SHA1Algorithm(nameof(CalculationLogic)),
            Algorithms.Algorithms.SHA256 => new SHA256Algorithm(nameof(CalculationLogic)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

        m_algorithm.SetWaitForKeypress(true);

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT,
            new EventListener($"{EventMaster.EVENT_ID_EXIT}.calculationLogic", ExitListener));
    }

    private void ExitListener(IEvent @event) => m_shouldExit = true;

    public void Start() => StartInMemory().Wait();

    public async Task<List<FileChecksum>> StartInMemory(bool shouldCallForExit = true, bool shouldSaveFile = true)
    {
        List<FileChecksum> checksums = [];

        switch (m_path)
        {
            case DirectoryInfo di:
            {
                PerformChecksumOnFiles(di, checksums);

                foreach (var dir in di.GetDirectories())
                {
                    if (m_shouldExit)
                        break;

                    PerformChecksumOnFiles(dir, checksums);
                }

                break;
            }
            case FileInfo fi:
                PerformCalculationOnFile(fi, checksums);

                break;
        }

        if (shouldSaveFile)
            FileWorker.SaveFile(new FileInfo($"{Directory.GetCurrentDirectory()}/checksum.dat").FullName,
                FileWorker.CreateSavedFile(m_path, m_usedAlgorithm, checksums));

        if (m_shouldOutput)
            GenerateReport(checksums);

        if (shouldCallForExit)
            EventMaster.Invoke(EventMaster.EVENT_ID_EXIT_CONFIRM, new EmptyEvent());

        return await Task.FromResult(checksums);
    }

    private void GenerateReport(List<FileChecksum> checksums)
    {
        IReport report = m_format switch
        {
            ReportTypes.Text => new TextReport(),
            ReportTypes.Json => new JsonReport(),
            _ => new TextReport()
        };

        Console.WriteLine($"\nReport:\n\n{report.CreateReport(checksums.ToArray())}");
    }

    private void PerformChecksumOnFiles(DirectoryInfo di, List<FileChecksum> checksums)
    {
        FileInfo[] files;

        try
        {
            files = di.GetFiles();
        }
        catch (Exception e)
        {
            return;
        }

        foreach (var file in files)
        {
            ConsoleInput.CheckForInput();

            PerformCalculationOnFile(file, checksums);

            if (m_shouldExit)
                break;

            ConsoleInput.CheckForInput();
        }
    }

    private void PerformCalculationOnFile(FileInfo file, List<FileChecksum> checksums)
    {
        ConsoleInput.CheckForInput();
        
        if(m_visited.Contains(file.FullName))
            return;

        if (m_shouldOutput)
            Console.WriteLine($"Processing: {file.FullName}");

        if (file.Attributes.HasFlag(FileAttributes.ReparsePoint)) //symlink
        {
            m_visited.Add(file.FullName);
            var fsi = ResolveSymlink(file);

            switch (fsi)
            {
                case FileInfo fi:
                    file = fi;

                    break;
                case DirectoryInfo di:
                    PerformChecksumOnFiles(di, checksums);

                    return;
            }
        }
        else if (file.Extension.EndsWith(".lnk")) //Windows Shortcut
        {
            m_visited.Add(file.FullName);
            var fsi = ResolveWindowsShortcut(file);

            switch (fsi)
            {
                case FileInfo fi:
                    file = fi;

                    break;
                case DirectoryInfo di:
                    PerformChecksumOnFiles(di, checksums);

                    foreach (var dir in di.GetDirectories()) 
                        PerformChecksumOnFiles(dir, checksums);

                    return;
            }
        }

        FileType type = IsBinaryFile(file) ? FileType.Binary : FileType.Other;

        Stream stream;
        try
        {
            stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            if (m_shouldOutput)
                Console.WriteLine($"\nError while opening file: `{file.FullName}`! Error: {e.Message}\n");

            return;
        }

        string checksum = m_algorithm.PerformAlgorithm(stream);

        stream.Close();

        if (checksum.Length == 0) //skip if there was an error creating the hash.
        {
            if (m_shouldOutput)
                Console.WriteLine($"\nError while calculating the checksum for file: `{file.FullName}`!\n");

            return;
        }
        
        m_visited.Add(file.FullName);

        if (m_shouldOutput)
            Console.WriteLine("\t\tOK!\n");

        checksums.Add(new FileChecksum(file.FullName, type, checksum));
    }

    private FileSystemInfo ResolveWindowsShortcut(FileInfo file)
    {
        try
        {
            var lnkFile = Lnk.Lnk.LoadFile(file.FullName);
            
            string targetPath = lnkFile.LocalPath;

            if (string.IsNullOrEmpty(targetPath))
                return file;
            
            if (File.Exists(targetPath))
            {
                return new FileInfo(targetPath);
            }

            if (Directory.Exists(targetPath))
            {
                return new DirectoryInfo(targetPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError resolving shortcut: {ex.Message}\n");
        }

        return file;
    }

    private FileSystemInfo ResolveSymlink(FileInfo file)
    {
        try
        {
            return file.ResolveLinkTarget(true) ?? file;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError resolving symbolic link: {ex.Message}\n");
        }

        return file;
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