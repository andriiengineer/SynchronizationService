using SynchronizationService.Interfaces;

namespace SynchronizationService.Core;

public class FileSystemOperations : IFileOperations
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
    
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }
    
    public void CopyFile(string source, string replica)
    {
        File.Copy(source, replica, overwrite: true);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }
    
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void DeleteDirectory(string path)
    {
        Directory.Delete(path, recursive: true);
    }
    
    public IEnumerable<string> GetFiles(string path)
    {
        return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
    }

    public IEnumerable<string> GetDirectories(string path)
    {
        return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
    }
}