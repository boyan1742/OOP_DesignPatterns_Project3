using System.Text;

using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Reports;

namespace OOP_DesignPatterns_Project3.Modes;

public sealed class VerificationLogic : IOperationLogic
{
    private readonly SavedFile m_savedFile;
    private readonly CalculationLogic m_calculationLogic;
    private readonly bool m_isInitialized;

    private DateTime m_lastPrint = DateTime.Now;

    public VerificationLogic(FileSystemInfo path, FileInfo checksums)
    {
        var file = FileWorker.LoadFile(checksums.FullName);

        if (file is null)
        {
            Console.WriteLine(
                "\nThere was an error loading the file! The program cannot continue! \nThis could indicate wrong file format or corruption!");

            m_isInitialized = false;

            return;
        }

        m_savedFile = file;

        FileSystemInfo? realPath = null;

        switch (path)
        {
            case DirectoryInfo when path.FullName != m_savedFile.LocationOfOperation &&
                                    path.FullName != Directory.GetCurrentDirectory():
                Console.WriteLine(
                    "\nThe specified directory with --path doesn't match the directory of the checksum file. The program cannot continue!");

                m_isInitialized = false;

                return;
            case DirectoryInfo when path.FullName == Directory.GetCurrentDirectory():
                realPath = new DirectoryInfo(m_savedFile.LocationOfOperation);

                break;
            case DirectoryInfo when path.FullName != Directory.GetCurrentDirectory():
                realPath = path;

                break;
            case FileInfo fi:
                bool doFileCheck = m_savedFile.Checksums.Any(x => x.Path == fi.FullName);

                if (doFileCheck)
                    realPath = path;

                break;
        }

        if (realPath is null)
        {
            Console.WriteLine("Couldn't determine the path where the operation should take place!");

            m_isInitialized = false;

            return;
        }

        m_calculationLogic = new CalculationLogic(realPath,
            m_savedFile.AlgorithmUsed, ReportTypes.Text, false);
        m_isInitialized = true;
    }

    public void Start()
    {
        if (!m_isInitialized)
            return;

        var newChecksums = Task.Run(() => m_calculationLogic.StartInMemory(false, false));

        Console.Write("Calculating checksums");
        while (!newChecksums.IsCompleted)
        {
            if ((DateTime.Now - m_lastPrint).Milliseconds < 250)
                continue;

            m_lastPrint = DateTime.Now;
            Console.Write(".");
        }

        Console.WriteLine();

        List<(FileChecksum, ChecksumStatus)> allFiles = [];

        foreach (var checksum in m_savedFile.Checksums)
        {
            ChecksumStatus status;

            FileChecksum? checksumFromChecksumFile = newChecksums.Result.Find(
                x => x.Path == checksum.Path);
            if (checksumFromChecksumFile is null)
                status = ChecksumStatus.Removed;
            else if (checksumFromChecksumFile.Checksum != checksum.Checksum)
                status = ChecksumStatus.Modified;
            else
                status = ChecksumStatus.Ok;

            allFiles.Add((checksum, status));
        }

        foreach (var checksum in newChecksums.Result)
        {
            if (m_savedFile.Checksums.Any(x => x.Path == checksum.Path))
                continue;

            allFiles.Add((checksum, ChecksumStatus.New));
        }

        allFiles.Sort((x, x1) =>
            StringComparer.OrdinalIgnoreCase.Compare(x.Item1.Path, x1.Item1.Path));

        foreach (var file in allFiles)
            Console.WriteLine($"{file.Item1.Path}: {file.Item2.ToString().ToUpper()}");

        EventMaster.Invoke(EventMaster.EVENT_ID_EXIT_CONFIRM, new EmptyEvent());
    }
}