namespace OOP_DesignPatterns_Project3.Algorithms;

public interface IChecksumAlgorithm
{
    void SetWaitForKeypress(bool value);
    string PerformAlgorithm(Stream fileStream);
}