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
            Console.WriteLine("Example for Windows: C:\\Source C:\\Replica 60 C:\\Logs\\sync.log");
            Console.WriteLine("Example for Linux/MacOS: /Source /Replica 60 /Logs/sync.log");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            return;
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        var logFilePath = args[3];

        if (!int.TryParse(args[2], out int intervalSeconds) || intervalSeconds <= 0)
        {
            Console.WriteLine("Error: Incorrect interval seconds value");
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
            services.AddSingleton<ILogger>(s => new FileAndConsoleLogger(s.GetRequiredService<IFileOperations>(), logFilePath));
            services.AddTransient<IFolderSync, FolderSync>();
            
            
            services.AddSingleton<SynchronizationProcess>();
            
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var service = serviceProvider.GetRequiredService<SynchronizationProcess>();

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    service.Stop();
                };

                service.Start();
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Unauthorized Access Exception: {ex.Message}");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}