using System.Diagnostics;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.Logging;

namespace QCommon.Internal.Logging;

internal sealed class PluginLoggerProvider(IPluginLogger actualLogger) : ILoggerProvider
{
    private readonly PluginLoggerHolder holder = new(actualLogger);
    public ILogger CreateLogger(string categoryName) => new Logger(holder, categoryName);

    public void Dispose() => holder.ClearLogger();

    private sealed class PluginLoggerHolder(IPluginLogger actualLogger) : IPluginLogger
    {
        private volatile IPluginLogger? actualLogger = actualLogger;

        public void Info(string message) => actualLogger?.Info(message);

        public void Warn(string message) => actualLogger?.Warn(message);

        public void Error(string message, Exception? exception = null) => actualLogger?.Error(message, exception);

        //public void SetLogger(IPluginLogger log) => Interlocked.Exchange(ref actualLogger, log);
        public void ClearLogger() => Interlocked.Exchange(ref actualLogger, null);
    }

    private sealed class LoggerT<T>(IPluginHost host) : ILogger<T>
    {
        private readonly Logger core = new(host.Logger, LoggerUtil.GetCategoryName<T>());
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => core.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => core.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => core.Log(logLevel, eventId, state, exception, formatter);
    }

    private sealed class Logger(IPluginLogger pluginLogger, string name) : ILogger
    {
        private readonly IPluginLogger pluginLogger = pluginLogger;
        private readonly string name = name;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel)
        {
#if !DEBUG
            if (logLevel is < LogLevel.Information)
                return false;
#endif
            return logLevel is not LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = $"{name}{(eventId.Id is 0 ? string.Empty : $":({eventId})")} [{logLevel}]: {formatter.Invoke(state, exception?.Demystify())}";

            if (logLevel is LogLevel.Error or LogLevel.Critical)
                pluginLogger.Error(message);
            else if (logLevel is LogLevel.Warning)
                pluginLogger.Warn(message);
            else
                pluginLogger.Info(message);
        }
    }
#if false
    public static ILogger CreateLogger<T>(IPluginLogger? logger)
    {
        if (logger is null)
            return NullLogger<T>.Instance;
        return new Logger(logger, LoggerUtil.GetCategoryName<T>());
    }

    public static ILogger CreateLogger<T>(IPluginLogger? logger, string? suffix = null)
    {
        if (logger is null)
            return NullLogger<T>.Instance;
        return new Logger(logger, LoggerUtil.GetCategoryName<T>(suffix));
    }

    public static ILogger CreateLogger(IPluginLogger? logger, string categoryName)
        => logger is null ? NullLogger.Instance : new Logger(logger, categoryName);
#endif
}