using Microsoft.Extensions.Logging;

namespace QPlug;

public static class LoggerExtensions
{
    private static bool IsEnabled([NotNullWhen(true)] ILogger? log, LogLevel logLevel) => log?.IsEnabled(logLevel) is true;
    extension ([NotNullWhen(true)] ILogger? log)
    {
        public bool TraceEnabled => IsEnabled(log, LogLevel.Trace);

        public bool DebugEnabled => IsEnabled(log, LogLevel.Debug); 

        public bool InformationEnabled => IsEnabled(log, LogLevel.Information);

        public bool WarningEnabled => IsEnabled(log, LogLevel.Warning);

        public bool ErrorEnabled => IsEnabled(log, LogLevel.Error);

        public bool CriticalEnabled => IsEnabled(log, LogLevel.Critical);
    }
}