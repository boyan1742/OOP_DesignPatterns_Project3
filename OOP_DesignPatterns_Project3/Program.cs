using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;

using OOP_DesignPatterns_Project3.Events;

namespace OOP_DesignPatterns_Project3;

class Program
{
    private static int m_lastUpdateProgress = 0;
    private static DateTime m_lastInvocation = DateTime.Now;
    private static bool m_confirmedExit = false;

    static async Task<int> Main(string[] args)
    {
        DirectoryInfo? checksumPath = null;
        Algorithms.Algorithms? checksumAlgorithm = null;
        FileInfo? checksumFile = null;

        var pathOption = new Option<DirectoryInfo>(
            name: "--path",
            description: "The path to the file or directory that will perform the checksum operation.",
            getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory())
        );

        var algorithmOption = new Option<Algorithms.Algorithms?>(
            name: "--algorithm",
            description: "The algorithm used to checksum the files."
        );

        var checksumsOption = new Option<FileInfo?>(
            name: "--checksums",
            description: "The file containing all checksums of the files scanned previously."
        );

        var rootCommand = new RootCommand("File checksum application.");

        rootCommand.AddOption(pathOption);
        rootCommand.AddOption(algorithmOption);
        rootCommand.AddOption(checksumsOption);

        //rootCommand.SetHandler(x => checksumPath = x, pathOption);
        //rootCommand.SetHandler(x => checksumAlgorithm = x, algorithmOption);
        //rootCommand.SetHandler(x => checksumFile = x, checksumsOption);

        rootCommand.SetHandler((path, algorithm, checksums) =>
        {
            if (algorithm != null && checksums != null)
            {
                Console.Error.WriteLine("Error: --algorithm and --checksums cannot be used together.");

                return;
            }

            if (algorithm != null)
            {
                if (algorithm == Algorithms.Algorithms.None)
                {
                    Console.Error.WriteLine("Error: value `None` for --algorithm is not valid.");

                    return;
                }

                checksumAlgorithm = algorithm;
            }
            else if (checksums != null)
            {
                checksumFile = checksums;
            }
            else
            {
                Console.WriteLine("No algorithm or checksums specified.");
            }

            checksumPath = path;
        }, pathOption, algorithmOption, checksumsOption);

        int exitCode = rootCommand.Invoke(args);

        if (exitCode != 0 ||
            (args.Length == 1 && (args[0] == "-?" || args[0] == "-h" || args[0] == "--help" ||
                                  args[0] == "--version")))
            return exitCode;

        EventMaster.Bind(EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE,
            new EventListener($"{EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE}.program",
                UpdateFileProgressListener));

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT_CONFIRM,
            new EventListener($"{EventMaster.EVENT_ID_EXIT_CONFIRM}.program",
                _ => m_confirmedExit = true));

        MasterControl control;
        try
        {
            control = new MasterControl(checksumPath ?? new DirectoryInfo(Directory.GetCurrentDirectory()),
                checksumAlgorithm ?? Algorithms.Algorithms.None,
                checksumFile ?? new FileInfo(Assembly.GetExecutingAssembly().Location));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return -1;
        }

        control.Start();

        while (!m_confirmedExit) ;

        return 0;
    }

    static void UpdateFileProgressListener(IEvent @event)
    {
        if (@event is not FileProgressUpdateEvent evn)
            return;

        if ((DateTime.Now - m_lastInvocation).TotalMilliseconds < 0.5)
            return;

        if (m_lastUpdateProgress == evn.GetProgress())
            return;

        Console.Write($" {evn.GetProgress()}%");
        m_lastUpdateProgress = evn.GetProgress();
        m_lastInvocation = DateTime.Now;
    }
}