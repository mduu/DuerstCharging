namespace DuerstCharging.Core.Logging;

public class LogReader : ILogReader
{
    public async Task<IEnumerable<string>> ReadLog(CancellationToken cancellationToken)
    {
        var path = Path.Combine(Environment.CurrentDirectory, "logs");
        var file = Directory.EnumerateFileSystemEntries(
            path,
            "*.log",
            new EnumerationOptions
            {
                ReturnSpecialDirectories = false,
            }).MaxBy(f => f);

        if (file is null)
        {
            return Enumerable.Empty<string>();
        }

        return await File.ReadAllLinesAsync(file, cancellationToken);
    }
}