using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace QCommon.Utils.Extensions.Logging;

public static class LogDurationScopeExtensions
{
    extension(ILogger log)
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(LogLevel logLevel = LogLevel.Trace, [CallerMemberName] string callerName = null!)
            => log.TrackDurationImpl(Stopwatch.GetTimestamp(), callerName, default, logLevel);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(EventId eventId, LogLevel logLevel = LogLevel.Trace, [CallerMemberName] string callerName = null!)
            => log.TrackDurationImpl(Stopwatch.GetTimestamp(), callerName, eventId, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(string name, LogLevel logLevel = LogLevel.Trace)
            => log.TrackDurationImpl(Stopwatch.GetTimestamp(), name, default, logLevel);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogDurationScope TrackDuration(string name, EventId eventId, LogLevel logLevel = LogLevel.Trace)
            => log.TrackDurationImpl(Stopwatch.GetTimestamp(), name, eventId, logLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LogDurationScope TrackDurationImpl(long startedAt, string name, EventId eventId, LogLevel logLevel = LogLevel.Trace)
        {
            if (log.IsEnabled(logLevel))
                return new LogDurationScope(log, startedAt, name, eventId, logLevel);
            else
                return default;
        }
    }
}
