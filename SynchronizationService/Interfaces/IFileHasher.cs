namespace SynchronizationService.Interfaces;

public interface IFileHasher
{
    string ComputeHash(string filePath);
}