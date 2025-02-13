using System.Text.Json;

using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

public static class FileWorker
{
    public static bool SaveBinary { get; set; } = true;

    public static SavedFile CreateSavedFile(FileSystemInfo info, Algorithms.Algorithms usedAlgorithm,
        List<FileChecksum> checksums) =>
        new(usedAlgorithm, info.FullName, checksums.ToArray());

    public static void SaveMemento(string location, MementoFile file)
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

        SaveMemento(fileStream, file);

        fileStream.Close();
    }

    public static void SaveMemento(Stream location, MementoFile file) => MessagePackSerializer.Serialize(location, file);

    public static MementoFile? LoadMemento(string location)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return null;
        }

        var deserializedFile = LoadMemento(fileStream);

        fileStream.Close();

        return deserializedFile;
    }

    public static MementoFile LoadMemento(Stream location) => MessagePackSerializer.Deserialize<MementoFile>(location);

    public static void SaveFile(string location, SavedFile file)
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

        SaveFile(fileStream, file);
        fileStream.Close();
    }

    public static void SaveFile(Stream location, SavedFile file)
    {
        if (SaveBinary)
            SaveFileBinary(location, file);
        else
            SaveFileJson(location, file);
    }

    public static SavedFile? LoadFile(string location)
    {
        Stream fileStream;
        try
        {
            fileStream = new FileStream(location, FileMode.Open, FileAccess.Read);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while trying to write to file! Error: {e.Message}");

            return null;
        }

        SavedFile? file = LoadFile(fileStream);

        return file;
    }

    public static SavedFile? LoadFile(Stream location) => SaveBinary ? LoadFileBinary(location) : LoadFileJson(location);

    private static void SaveFileJson(Stream fileStream, SavedFile file)
    {
        JsonSerializer.Serialize(fileStream, file, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static void SaveFileBinary(Stream fileStream, SavedFile file)
    {
        MessagePackSerializer.Serialize(fileStream, file);
    }

    private static SavedFile? LoadFileJson(Stream fileStream)
    {
        SavedFile? file = null;

        try
        {
            file = JsonSerializer.Deserialize<SavedFile>(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured when deserializing the checksum file! Error: {e.Message}");
        }

        return file;
    }

    private static SavedFile? LoadFileBinary(Stream fileStream)
    {
        SavedFile? file = null;

        try
        {
            file = MessagePackSerializer.Deserialize<SavedFile>(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured when deserializing the checksum file! Error: {e.Message}");
        }

        return file;
    }
}