using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

[MessagePackObject(true)]
public sealed class FileChecksum
{
    public string Path { get; init; }
    public FileType Type { get; init; }
    public string Checksum { get; init; }
    
    public FileChecksum(string path, FileType type, string checksum)
    {
        Path = path;
        Type = type;
        Checksum = checksum;
    }
}