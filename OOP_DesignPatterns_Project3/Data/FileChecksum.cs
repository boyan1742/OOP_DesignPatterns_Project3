using System.Text.Json.Serialization;

using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

[MessagePackObject(true)]
public sealed class FileChecksum
{
    public string Path { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<FileType>))]
    public FileType Type { get; init; }
    public string Checksum { get; init; }
    
    public FileChecksum(string path, FileType type, string checksum)
    {
        Path = path;
        Type = type;
        Checksum = checksum;
    }
    
    public static bool operator ==(FileChecksum lhs, FileChecksum rhs) =>
        lhs.Path == rhs.Path &&
        lhs.Type == rhs.Type && lhs.Checksum == rhs.Checksum;

    public static bool operator !=(FileChecksum lhs, FileChecksum rhs) => !(lhs == rhs);
}