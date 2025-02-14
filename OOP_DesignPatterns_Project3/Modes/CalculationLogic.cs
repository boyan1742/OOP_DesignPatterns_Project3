using System.Runtime.InteropServices;

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
    private bool m_paused = false;

    private readonly Dictionary<string, bool> m_visited = [];
    private readonly List<FileChecksum> m_checksums = [];

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

        EventMaster.Bind(EventMaster.EVENT_ID_PAUSE,
            new EventListener($"{EventMaster.EVENT_ID_PAUSE}.calculationLogic", PauseListener));
    }

    private void ExitListener(IEvent @event) => m_shouldExit = true;

    private void PauseListener(IEvent @event)
    {
        m_paused = !m_paused;

        if (m_paused)
        {
            var file = new MementoFile(m_checksums.ToArray(),
                m_visited.Where(x => !x.Value)
                    .Select(x => x.Key).ToArray(), m_path.FullName);

            FileWorker.SaveMemento("./memento.dat", file);
        }
        else
        {
            MementoFile? file = FileWorker.LoadMemento("./memento.dat");
            if (file is null)
            {
                if(m_shouldOutput)
                    Console.WriteLine("\nThere was an error reading file!");
                
                return;
            }

            if (m_path.FullName != file.StartDirectory)
            {
                if(m_shouldOutput)
                    Console.WriteLine("\nThe restore cannot complete! Incompatible folders!");
                
                return;
            }
            
            m_checksums.Clear();
            m_checksums.AddRange(file.Finished);

            foreach (var finished in m_checksums)
            {
                if(m_visited.ContainsKey(finished.Path))
                    m_visited[finished.Path] = true;
            }
            
            foreach (var notFinished in file.NotFinished)
            {
                if(m_visited.ContainsKey(notFinished))
                    m_visited[notFinished] = false;
            }
        }
    }

    public void Start() => StartInMemory().Wait();

    public async Task<List<FileChecksum>> StartInMemory(bool shouldCallForExit = true, bool shouldSaveFile = true)
    {
        if(m_shouldOutput)
            Console.WriteLine("\nCaching entries!\n\n");
        
        CacheEntries();

        CalculateChecksums(m_checksums);

        if (shouldSaveFile)
            FileWorker.SaveFile(new FileInfo($"{Directory.GetCurrentDirectory()}/checksum.dat").FullName,
                FileWorker.CreateSavedFile(m_path, m_usedAlgorithm, m_checksums));

        if (m_shouldOutput)
            GenerateReport(m_checksums);

        if (shouldCallForExit)
            EventMaster.Invoke(EventMaster.EVENT_ID_EXIT_CONFIRM, new EmptyEvent());

        return await Task.FromResult(m_checksums);
    }

    private void CalculateChecksums(List<FileChecksum> checksums)
    {
        long done = 0;
        foreach (var file in m_visited.Keys)
        {
            PerformCalculationOnFile(new FileInfo(file), checksums);

            if (m_paused)
            {
                while (m_paused)
                {
                    ConsoleInput.CheckForInput();
                }
                PerformCalculationOnFile(new FileInfo(file), checksums);
            }

            if (m_shouldExit)
                break;

            if (m_shouldOutput) 
                Console.WriteLine($"\n{++done}/{m_visited.Count}");
        }
    }

    private void CacheEntries()
    {
        m_visited.Clear();

        switch (m_path)
        {
            case FileInfo fi:
                m_visited.Add(fi.FullName, false);

                break;
            case DirectoryInfo di:
            {
                FindAllDirs(di);

                break;
            }
        }
    }

    private void FindAllDirs(DirectoryInfo di)
    {
        foreach (var fi in di.GetFiles())
            FindAllFiles(fi);

        foreach (var dir in di.GetDirectories())
            FindAllDirs(dir);
    }

    private void FindAllFiles(FileInfo file)
    {
        ref var entry = ref
            CollectionsMarshal.GetValueRefOrAddDefault(m_visited, file.FullName, out bool exists);

        if (exists)
            return;

        entry = false;

        FileSystemInfo? fsi = null;
        if (file.Attributes.HasFlag(FileAttributes.ReparsePoint)) //symlink
            fsi = ResolveSymlink(file);
        else if (file.Extension.EndsWith(".lnk")) //Windows Shortcut
            fsi = ResolveWindowsShortcut(file);

        switch (fsi)
        {
            case FileInfo fi:
                ref var entry2 = ref
                    CollectionsMarshal.GetValueRefOrAddDefault(m_visited, fi.FullName, out bool _);

                entry2 = false;

                break;
            case DirectoryInfo di1:
                FindAllDirs(di1);

                return;
        }
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

    private void PerformCalculationOnFile(FileInfo file, List<FileChecksum> checksums)
    {
        ConsoleInput.CheckForInput();

        if (m_visited[file.FullName])
            return;

        if (m_shouldOutput)
            Console.WriteLine($"Processing: {file.FullName}");

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

        m_visited[file.FullName] = true;

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