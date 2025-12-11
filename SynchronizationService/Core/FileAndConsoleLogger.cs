using System.Text;
using SynchronizationService.Interfaces;

namespace SynchronizationService.Core;

public class FileAndConsoleLogger : ILogger, IDisposable
{
    private readonly TextWriter _fileWriter;
    private readonly TextWriter _consoleWriter;

    public FileAndConsoleLogger(IFileOperations fileOperations, string logFilePath)
    {
        if (Directory.Exists(logFilePath))
        {
            throw new ArgumentException(
                $"The path points to a directory, but a file is needed: {logFilePath}\n" +
                $"Please try to use: {Path.Combine(logFilePath, "sync.log")}");
        }

        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            fileOperations.CreateDirectory(directory);
        }

        try
        {
            var fileStream = new StreamWriter(logFilePath, append: true, Encoding.UTF8) 
            { 
                AutoFlush = true
            };

            _fileWriter = TextWriter.Synchronized(fileStream);
            _consoleWriter = TextWriter.Synchronized(Console.Out);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"You have not access to file: {logFilePath}", ex);
        }
    }

    public void Log(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

        _consoleWriter.WriteLine(logEntry);
        _fileWriter.WriteLine(logEntry);
    }

    public void Dispose()
    {
        _fileWriter?.Dispose();
        GC.SuppressFinalize(this);
    }
}