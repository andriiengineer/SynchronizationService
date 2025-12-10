using Microsoft.Extensions.DependencyInjection;
using SynchronizationService.Core;
using SynchronizationService.DTO;
using SynchronizationService.Interfaces;

namespace SynchronizationService;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("FolderSync <source_path> <replica_path> <interval_Seconds> <log_file_path>");
            Console.WriteLine("Example windows: C:\\Source C:\\Replica 60 C:\\Logs\\sync.log");
            Console.WriteLine("Example linux/macos: /Source /Replica 60 /Logs/sync.log");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            return;
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        var logFilePath = args[3];

        if (!int.TryParse(args[2], out int intervalSeconds) || intervalSeconds <= 0)
        {
            Console.WriteLine("ERROR: Incorrect interval seconds value");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            return;
        }

        try
        {
            var services = new ServiceCollection();
            services.AddSingleton(new SynchronizationOptions
            {
                SourcePath = sourcePath,
                ReplicaPath = replicaPath,
                IntervalSeconds = intervalSeconds
            });
            
            services.AddSingleton<IFileHasher, XxHashFileHasher>();
            services.AddSingleton<IFileOperations, FileSystemOperations>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}