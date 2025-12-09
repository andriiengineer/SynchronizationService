namespace SynchronizationService;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine($"SourcePath = {args[0]}, ReplicaPath = {args[1]}, intervalTimeSeconds = {args[2]}, PathLogs = {args[3]}");
        Console.ReadKey();
    }
}