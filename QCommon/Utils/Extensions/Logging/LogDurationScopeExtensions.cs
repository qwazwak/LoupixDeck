using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace QCommon.Utils.Extensions.Logging;

public static class LogDurationScopeExtensions
{
    extension(ILogger? log)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(LogLevel logLevel = LogLevel.Trace, [CallerMemberName] string callerName = null!)
            => TrackDurationImpl(log, callerName, default, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(EventId eventId, LogLevel logLevel = LogLevel.Trace, [CallerMemberName] string callerName = null!)
            => TrackDurationImpl(log, callerName, eventId, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(string name, LogLevel logLevel = LogLevel.Trace)
            => TrackDurationImpl(log, name, default, logLevel);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(string name, EventId eventId, LogLevel logLevel = LogLevel.Trace)
            => TrackDurationImpl(log, name, eventId, logLevel);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LogDurationScope TrackDurationImpl(ILogger? log, string name, EventId eventId, LogLevel logLevel = LogLevel.Trace)
    {
        if (log is not null && log.IsEnabled(logLevel))
            return new LogDurationScope(log, Stopwatch.GetTimestamp(), name, eventId, logLevel);
        else
            return default;
    }
}
