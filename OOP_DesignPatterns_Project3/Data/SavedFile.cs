using System.Text.Json.Serialization;

using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

[MessagePackObject(true)]
public class SavedFile
{
    [JsonConverter(typeof(JsonStringEnumConverter<Algorithms.Algorithms>))]
    public Algorithms.Algorithms AlgorithmUsed { get; init; }
    public string LocationOfOperation { get; init; }
    public FileChecksum[] Checksums { get; init; }
    
    public SavedFile(Algorithms.Algorithms algorithmUsed, string locationOfOperation, FileChecksum[] checksums)
    {
        AlgorithmUsed = algorithmUsed;
        LocationOfOperation = locationOfOperation;
        Checksums = checksums;
    }

    public static bool operator ==(SavedFile lhs, SavedFile rhs)
    {
        if (lhs.Checksums.Length != rhs.Checksums.Length)
            return false;
        
        if (lhs.Checksums.Where((t, i) => t != rhs.Checksums[i]).Any())
            return false;
        
        return lhs.LocationOfOperation == rhs.LocationOfOperation &&
               lhs.AlgorithmUsed == rhs.AlgorithmUsed;
    }

    public static bool operator !=(SavedFile lhs, SavedFile rhs) => !(lhs == rhs);
}