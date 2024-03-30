namespace DuerstCharging.Core.Logging;

public interface ILogReader
{
    Task<IEnumerable<string>> ReadLog(CancellationToken cancellationToken);
}