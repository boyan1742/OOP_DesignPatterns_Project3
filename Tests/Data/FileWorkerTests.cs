using System.Text;
using System.Text.Json;

using MessagePack;

using OOP_DesignPatterns_Project3.Data;

using Alg = OOP_DesignPatterns_Project3.Algorithms.Algorithms;

namespace Tests.Data;

public class FileWorkerTests
{
    [Fact]
    public void TestSavingJsonFile()
    {
        FileWorker.SaveBinary = false;

        Stream stream = new MemoryStream(new byte[16 * 1024]);
        string dir = "xyz";

        var file = new SavedFile(Alg.MD5, dir, [
            new FileChecksum(dir, FileType.Binary, "abc"),
            new FileChecksum(dir, FileType.Other, "def"),
            new FileChecksum(dir, FileType.Binary, "ghi")
        ]);

        FileWorker.SaveFile(stream, file);
        stream.Flush();
        stream.Position = 0;

        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        string jsonString = streamReader.ReadToEnd().Trim(); // Trim any extra characters
        jsonString = jsonString.Trim('\0');

        var deserializedFile = JsonSerializer.Deserialize<SavedFile>(jsonString);

        Assert.NotNull(deserializedFile);
        Assert.True(file == deserializedFile);
    }

    [Fact]
    public void TestSavingBinaryFile()
    {
        FileWorker.SaveBinary = true;

        Stream stream = new MemoryStream(new byte[16 * 1024]);
        string dir = "xyz";

        var file = new SavedFile(Alg.MD5, dir, [
            new FileChecksum(dir, FileType.Binary, "abc"),
            new FileChecksum(dir, FileType.Other, "def"),
            new FileChecksum(dir, FileType.Binary, "ghi")
        ]);

        FileWorker.SaveFile(stream, file);
        stream.Flush();
        stream.Position = 0;

        var deserializedFile = MessagePackSerializer.Deserialize<SavedFile>(stream);

        Assert.NotNull(deserializedFile);
        Assert.True(file == deserializedFile);
    }

    [Fact]
    public void TestLoadingBinaryFile()
    {
        FileWorker.SaveBinary = true;

        Stream stream = new MemoryStream(new byte[16 * 1024]);
        string dir = "xyz";

        var file = new SavedFile(Alg.MD5, dir, [
            new FileChecksum(dir, FileType.Binary, "abc"),
            new FileChecksum(dir, FileType.Other, "def"),
            new FileChecksum(dir, FileType.Binary, "ghi")
        ]);

        FileWorker.SaveFile(stream, file);
        stream.Flush();
        stream.Position = 0;

        var deserializedFile = FileWorker.LoadFile(stream);

        Assert.NotNull(deserializedFile);
        Assert.True(file == deserializedFile);
    }
}