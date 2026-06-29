using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QCommon.Utils.Extensions.Logging;

public readonly struct LogDurationScope(ILogger log, long startedAt, string name, EventId eventId, LogLevel logLevel) : IDisposable
{
    private readonly ILogger log = log;
    private readonly long startedAt = startedAt;
    private readonly string name = name;
    private readonly EventId eventId = eventId;
    private readonly LogLevel logLevel = logLevel;
#if TODO
    private Exception? ex;
    private LogLevel exceptionLogLevel;

    public void AttachException(Exception ex, LogLevel logLevel = LogLevel.Error)
    {
        if(this.ex is not null)
        {
            log.LogError("An exception was already attached to this LogDurationScope, overwriting it with the new exception. Previous exception: {previousException}", this.ex);
        }
        this.ex = ex;
        this.exceptionLogLevel = logLevel;
    }
#endif

    public readonly void Dispose()
    {
        if (log is null)
            return;
        long endedAt = Stopwatch.GetTimestamp();
        LogEnd(endedAt);
    }

    public readonly void LogEnd(long endedAt)
    {
        if (log is null)
            return;
        LogEnd(log, startedAt, endedAt, name, eventId, logLevel);
    }

    public static void LogEnd(ILogger log, long startedAt, long endedAt, string name, EventId eventId, LogLevel logLevel)
    {
        TimeSpan duration = Stopwatch.GetElapsedTime(startedAt, endedAt);
        LogEnd(log, duration, name, eventId, logLevel);
    }

    public static void LogEnd(ILogger log, TimeSpan duration, string name, EventId eventId, LogLevel logLevel)
    {
        Debug.Assert(log is not null);
        log.Log(logLevel, eventId, "{name} completed in {duration} ({durationRaw})", name, FormatDuration(duration), duration);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        double durationRaw;
        string units;
        if (duration.Ticks >= TimeSpan.TicksPerMillisecond)
            (durationRaw, units) = (duration.TotalMilliseconds, "ms");
        else
            (durationRaw, units) = (duration.TotalMicroseconds, "µs");
        return durationRaw.ToString("0.##") + units;
    }
}
