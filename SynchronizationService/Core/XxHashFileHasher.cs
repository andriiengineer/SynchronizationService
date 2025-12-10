using System.IO.Hashing;
using SynchronizationService.Interfaces;

namespace SynchronizationService.Core;

public class XxHashFileHasher : IFileHasher
{
    public string ComputeHash(string filePath)
    {
        var hasher = new XxHash64();
        using var stream = File.OpenRead(filePath);
        hasher.Append(stream);
        return Convert.ToHexStringLower(hasher.GetCurrentHash());
    }
}