using System.Collections.Concurrent;
using System.Collections.Frozen;
using SynchronizationService.Interfaces;

using FileInfo = SynchronizationService.DTO.FileInfo;

namespace SynchronizationService.Core;

public class FolderSync(IFileHasher hasher, IFileOperations fileSystem, ILogger logger) : IFolderSync
{
    private readonly ConcurrentDictionary<string, bool> _createdDirs = new();

    public void Synchronize(string sourcePath, string replicaPath)
    {
        logger.Log($"Start Sync: {sourcePath} -> {replicaPath}");
        _createdDirs.Clear();

        if (!fileSystem.DirectoryExists(sourcePath))
        {
            logger.Log($"ERROR: Source not found: {sourcePath}");
            return;
        }

        EnsureDirectoryExists(replicaPath);
        SyncDirectoryStructure(sourcePath, replicaPath);

        FrozenDictionary<string, FileInfo> sourceFiles = null!;
        FrozenDictionary<string, FileInfo> replicaFiles = null!;

        Parallel.Invoke(
            () => sourceFiles = ScanFiles(sourcePath, sourcePath),
            () => replicaFiles = ScanFiles(replicaPath, replicaPath)
        );

        logger.Log($"Source files: {sourceFiles.Count} | Replica files: {replicaFiles.Count}");
        
        ProcessFileSynchronization(sourcePath, replicaPath, sourceFiles, replicaFiles);

        ProcessCleanup(sourcePath, replicaPath, sourceFiles, replicaFiles);

        logger.Log("<<<<<<< Sync completed >>>>>>>");
    }


    private void SyncDirectoryStructure(string sourcePath, string replicaPath)
    {

        var allDirectories = fileSystem.GetDirectories(sourcePath);

        Parallel.ForEach(allDirectories, dirPath =>
        {
            var relativePath = Path.GetRelativePath(sourcePath, dirPath);
            
            var targetDir = Path.Combine(replicaPath, relativePath);

            EnsureDirectoryExists(targetDir);
        });
    }

    private FrozenDictionary<string, FileInfo> ScanFiles(string rootPath, string basePath)
    {
        var results = new ConcurrentBag<FileInfo>();

        Parallel.ForEach(fileSystem.GetFiles(rootPath), 
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, 
            filePath =>
        {
            try
            {
                var relPath = Path.GetRelativePath(basePath, filePath);
                results.Add(new FileInfo 
                { 
                    FullPath = filePath, 
                    RelativePath = relPath, 
                    Hash = hasher.ComputeHash(filePath) 
                });
            }
            catch (Exception ex)
            {
                logger.Log($"Error reading {filePath}: {ex.Message}");
                logger.Log($"File {filePath} can not be sync to replica");
            }
        });

        return results.ToFrozenDictionary(x => x.RelativePath, StringComparer.OrdinalIgnoreCase);
    }

    private void ProcessFileSynchronization(string sourcePath, string replicaPath,
        FrozenDictionary<string, FileInfo> sourceFiles, 
        FrozenDictionary<string, FileInfo> replicaFiles)
    {
        Parallel.ForEach(sourceFiles.Values, srcFile =>
        {
            var targetPath = Path.Combine(replicaPath, srcFile.RelativePath);
            
            bool exists = replicaFiles.TryGetValue(srcFile.RelativePath, out var destFile);
            
            if (!exists || destFile.Hash != srcFile.Hash)
            {
                try
                {
                    EnsureDirectoryExists(Path.GetDirectoryName(targetPath));
                    
                    fileSystem.CopyFile(srcFile.FullPath, targetPath);
                    
                    var action = exists ? "Updated" : "Copied";
                    logger.Log($"[{action}] {srcFile.RelativePath}");
                }
                catch (Exception ex)
                {
                    logger.Log($"Copy Error {srcFile.RelativePath}: {ex.Message}");
                }
            }
        });
    }

    private void ProcessCleanup(string sourcePath, string replicaPath,
        FrozenDictionary<string, FileInfo> sourceFiles, 
        FrozenDictionary<string, FileInfo> replicaFiles)
    {
        var toDelete = replicaFiles.Values
            .Where(r => !sourceFiles.ContainsKey(r.RelativePath));

        Parallel.ForEach(toDelete, file =>
        {
            try
            {
                fileSystem.DeleteFile(file.FullPath);
                logger.Log($"Deleted file: {file.RelativePath}");
            }
            catch (Exception ex)
            {
                logger.Log($"Delete Error: {ex.Message}");
            }
        });
        
        var dirs = fileSystem.GetDirectories(replicaPath)
            .OrderByDescending(d => d.Length);

        foreach (var dir in dirs)
        {
            var relPath = Path.GetRelativePath(replicaPath, dir);
            var srcDir = Path.Combine(sourcePath, relPath);

            if (!fileSystem.DirectoryExists(srcDir))
            {
                try
                {
                    if (!fileSystem.GetFiles(dir).Any() && !fileSystem.GetDirectories(dir).Any())
                    {
                        fileSystem.DeleteDirectory(dir);
                        logger.Log($"Deleted dir: {relPath}");
                    }
                }
                catch
                {
                    logger.Log("Error: Looks like access denied.");
                }
            }
        }
    }

    private void EnsureDirectoryExists(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        
        if (_createdDirs.TryAdd(path, true))
        {
            if (!fileSystem.DirectoryExists(path))
            {
                fileSystem.CreateDirectory(path);
            }
        }
    }
}