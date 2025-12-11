using SynchronizationService.DTO;
using SynchronizationService.Interfaces;

namespace SynchronizationService.Core;

public class SynchronizationProcess(
    IFolderSync synchronizer, 
    ILogger logger,
    SynchronizationOptions options)
{
    private bool _isRunning;

    public void Start()
    {
        _isRunning = true;
        logger.Log("<<<<<<< Sync running >>>>>>>");
        logger.Log($"Interval: {options.IntervalSeconds} seconds");
        logger.Log("Press Ctrl+C for stopping and await...");

        while (_isRunning)
        {
            try
            {
                synchronizer.Synchronize(options.SourcePath, options.ReplicaPath);
            }
            catch (Exception ex)
            {
                logger.Log($"Critical error during sync: {ex.Message}");
            }

            Thread.Sleep(options.IntervalSeconds * 1000);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        logger.Log("<<<<<<< Sync stopped >>>>>>>");
    }
}