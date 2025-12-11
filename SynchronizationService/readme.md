## Quick Start

### Running Synchronization

Navigate to the project folder and execute the command:

```bash
dotnet run "<path_to_source_folder>" "<path_to_replica_folder>" <interval_seconds> "<path_to_log_file>"
```

**macOS/Linux:**
```bash
dotnet run "/Users/Source" "/Users/andrii/Documents/Replica" 60 "/Users/Documents/Logs/sync.log"
```

**Windows:**
```powershell
dotnet run "C:\Documents\TestSource" "D:\TestReplica" 60 "C:\Logs\sync.log"
```

**Parameters:**
1. Source folder path
2. Replica folder path
3. Synchronization interval (seconds)
4. Log file path

**Stop:** Press `Ctrl+C`

---

## Why xxHash Instead of MD5?

I use **xxHash64** instead of MD5 for maximum performance:

![xxHash vs MD5 Performance](https://www.alexeyfv.xyz/_astro/benchmark.p1a9ohrQ_14tw8J.webp)