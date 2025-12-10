namespace SynchronizationService.DTO;

public class SynchronizationOptions
{
    public string SourcePath { get; set; }
    public string ReplicaPath { get; set; }
    public int IntervalSeconds { get; set; }
}