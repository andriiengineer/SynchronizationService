namespace SynchronizationService.Interfaces;

public interface IFolderSync
{
    void Synchronize(string sourcePath, string replicaPath);
}