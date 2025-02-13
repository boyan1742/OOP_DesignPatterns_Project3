using System.Text.Json;

using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

public static class FileWorker
{
    public static bool SaveBinary = true;

    public static SavedFile CreateSavedFile(FileSystemInfo info, Algorithms.Algorithms usedAlgorithm,
        List<FileChecksum> checksums) =>
        new(usedAlgorithm, info.FullName, checksums.ToArray());

    public static void SaveFile(string location, SavedFile file)
    {
        if (SaveBinary)
            SaveFileBinary(location, file);
        else
            SaveFileJson(location, file);
    }

    public static SavedFile? LoadFile(string location)
    {
        if (SaveBinary)
            return LoadFileBinary(location);
        else
            return LoadFileJson(location);
    }

    private static void SaveFileJson(string location, SavedFile file)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.OpenOrCreate, FileAccess.Write);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return;
        }

        JsonSerializer.Serialize(fileStream, file, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        fileStream.Close();
    }

    private static void SaveFileBinary(string location, SavedFile file)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.OpenOrCreate, FileAccess.Write);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return;
        }

        MessagePackSerializer.Serialize(fileStream, file);
        fileStream.Close();
    }

    private static SavedFile? LoadFileJson(string location)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.Open, FileAccess.Write);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return null;
        }

        SavedFile? file = null;

        try
        {
            file = JsonSerializer.Deserialize<SavedFile>(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured when deserializing the checksum file! Error: {e.Message}");
        }

        fileStream.Close();

        return file;
    }

    private static SavedFile? LoadFileBinary(string location)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.Open, FileAccess.Write);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return null;
        }

        SavedFile? file = null;

        try
        {
            file = MessagePackSerializer.Deserialize<SavedFile>(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured when deserializing the checksum file! Error: {e.Message}");
        }

        fileStream.Close();

        return file;
    }
}