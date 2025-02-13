using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

[MessagePackObject(true)]
public class SavedFile
{
    public Algorithms.Algorithms AlgorithmUsed { get; init; }
    public string LocationOfOperation { get; init; }
    public FileChecksum[] Checksums { get; init; }
    
    public SavedFile(Algorithms.Algorithms algorithmUsed, string locationOfOperation, FileChecksum[] checksums)
    {
        AlgorithmUsed = algorithmUsed;
        LocationOfOperation = locationOfOperation;
        Checksums = checksums;
    }
}