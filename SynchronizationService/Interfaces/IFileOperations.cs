namespace SynchronizationService.Interfaces;

public interface IFileOperations
{
    bool FileExists(string filePath);
    bool DirectoryExists(string path);
    void CopyFile(string sourcePath, string replicaPath);
    void DeleteFile(string filePath);
    void CreateDirectory(string path);
    void DeleteDirectory(string path);
    IEnumerable<string> GetFiles(string path);
    IEnumerable<string> GetDirectories(string path);
}