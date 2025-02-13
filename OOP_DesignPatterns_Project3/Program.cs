﻿using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;

using OOP_DesignPatterns_Project3.Data;
using OOP_DesignPatterns_Project3.Events;
using OOP_DesignPatterns_Project3.Modes;

namespace OOP_DesignPatterns_Project3;

class Program
{
    private static int m_lastUpdateProgress = 0;
    private static DateTime m_lastInvocation = DateTime.Now;
    private static bool m_confirmedExit = false;

    private static FileSystemInfo? m_checksumPath = null;
    private static Algorithms.Algorithms? m_checksumAlgorithm = null;
    private static FileInfo? m_checksumFile = null;

    private static int Main(string[] args)
    {
        RootCommand rootCommand = SetupCommandLineArguments();
        int exitCode = rootCommand.Invoke(args);

        if (exitCode != 0 ||
            (args.Length == 1 && (args[0] == "-?" || args[0] == "-h" || args[0] == "--help" ||
                                  args[0] == "--version")) || !(m_checksumPath?.Exists ?? false))
            return ExitWithCode(exitCode);

        EventMaster.Bind(EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE,
            new EventListener($"{EventMaster.EVENT_ID_FILE_PROGRESS_UPDATE}.program",
                UpdateFileProgressListener));

        EventMaster.Bind(EventMaster.EVENT_ID_EXIT_CONFIRM,
            new EventListener($"{EventMaster.EVENT_ID_EXIT_CONFIRM}.program",
                _ => m_confirmedExit = true));

        FileWorker.SaveBinary = false; //TODO: remove when finished. Only for testing.

        MasterControl control;
        try
        {
            control = new MasterControl(m_checksumPath ?? new DirectoryInfo(Directory.GetCurrentDirectory()),
                m_checksumAlgorithm ?? Algorithms.Algorithms.None,
                m_checksumFile ?? new FileInfo(Assembly.GetExecutingAssembly().Location));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            return ExitWithCode(-1);
        }

        control.Start();

        while (!m_confirmedExit) ;

        return ExitWithCode(0);
    }

    private static RootCommand SetupCommandLineArguments()
    {
        var pathOption = new Option<FileSystemInfo>(
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

        rootCommand.SetHandler((path, algorithm, checksums) =>
        {
            if (algorithm != null && checksums != null)
            {
                Console.Error.WriteLine("Error: --algorithm and --checksums cannot be used together!");

                return;
            }

            if (algorithm != null)
            {
                if (algorithm == Algorithms.Algorithms.None)
                {
                    Console.Error.WriteLine("Error: value `None` for --algorithm is not valid!");

                    return;
                }

                m_checksumAlgorithm = algorithm;
            }
            else if (checksums != null)
            {
                m_checksumFile = checksums;
            }
            else
            {
                Console.Error.WriteLine("No algorithm or checksums specified!");

                return;
            }

            if (!path.Exists)
            {
                Console.Error.WriteLine($"The specified path doesn't exist! Path: `{path.FullName}`");

                return;
            }

            m_checksumPath = path;
        }, pathOption, algorithmOption, checksumsOption);

        return rootCommand;
    }

    private static int ExitWithCode(int code)
    {
        Console.WriteLine("\n\nPress any key to exit!");
        Console.ReadKey();

        return code;
    }

    private static void UpdateFileProgressListener(IEvent @event)
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