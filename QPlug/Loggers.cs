using System.Diagnostics;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static QPlug.Loggers;

namespace QPlug;

internal static class Loggers
{
    private static string GetCategoryName<T>() => TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: true, includeGenericParameterNames: false/*, nestedTypeDelimiter: '.'*/);
    private static string GetCategoryName<T>(string? suffix)
    {
        string baseName = GetCategoryName<T>();
        return suffix is not null ? $"{baseName}.{suffix}" : baseName;
    }

    public static ILogger CreateLogger<T>(IPluginLogger? logger)
    {
        if (logger is null)
            return NullLogger<T>.Instance;
        return new PluginLoggerAdapter(logger, GetCategoryName<T>());
    }

    public static ILogger CreateLogger<T>(IPluginLogger? logger, string? suffix = null)
    {
        if (logger is null)
            return NullLogger<T>.Instance;
        return new PluginLoggerAdapter(logger, GetCategoryName<T>(suffix));
    }

    public static ILogger CreateLogger(IPluginLogger? logger, string categoryName)
        => logger is null ? NullLogger.Instance : new PluginLoggerAdapter(logger, categoryName);

    internal sealed class LoggerT<T>(IPluginHost host) : ILogger<T>
    {
        private readonly PluginLoggerAdapter core = new(host.Logger, GetCategoryName<T>());
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => core.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => core.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => core.Log(logLevel, eventId, state, exception, formatter);
    }
    private sealed class PluginLoggerAdapter(IPluginLogger pluginLogger, string name) : ILogger
    {
        private readonly IPluginLogger pluginLogger = pluginLogger;
        private readonly string name = name;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
#if DEBUG
            => true;
#else
        => logLevel is not LogLevel.None and > LogLevel.Information;
#endif

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel is LogLevel.None)
                return;
#if !DEBUG
        if (logLevel is LogLevel.Debug or LogLevel.Trace)
            return;
#endif
            string messageBase = $"{name}{(eventId.Id is 0 ? string.Empty : $":({eventId})")} [{logLevel}]: ";
            if (logLevel is LogLevel.Error or LogLevel.Critical)
            {
                string message = formatter.Invoke(state, null);
                pluginLogger.Error(messageBase + message, exception?.Demystify());
            }
            else
            {
                string message = messageBase + formatter.Invoke(state, exception?.Demystify());
                if (logLevel is LogLevel.Warning)
                    pluginLogger.Warn(message);
                else
                    pluginLogger.Info(message);
            }
        }
    }
}