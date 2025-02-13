using MessagePack;

namespace OOP_DesignPatterns_Project3.Data;

[MessagePackObject(true)]
public class MementoFile
{
    public FileChecksum[] Finished { get; }
    public string[] NotFinished { get; }
    public string StartDirectory { get; }

    public MementoFile(FileChecksum[] finished, string[] notFinished, string startDirectory)
    {
        Finished = new FileChecksum[finished.Length];
        Array.Copy(finished, Finished, finished.Length);
        
        NotFinished = new string[notFinished.Length];
        Array.Copy(notFinished, NotFinished, notFinished.Length);

        StartDirectory = startDirectory;
    }
}