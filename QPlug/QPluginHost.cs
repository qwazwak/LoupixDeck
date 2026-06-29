using LoupixDeck.PluginSdk;

namespace QPlug;

public sealed class QPluginHost : LoupixPlugin, IMenuContributor
{
    private QPlugin? impl;
    private QPlugin Impl => impl ?? throw new InvalidOperationException("Plugin not initialized");

    public override PluginMetadata Metadata => QPlugin.Metadata;
    public override void Initialize(IPluginHost host)
    {
        //SetAllLoggers(host.Logger);
        impl = new(host);
    }

    public override void Shutdown()
    {
        Interlocked.Exchange(ref impl, null)?.Dispose();
        //ClearAllLoggers();
    }

    public override IEnumerable<IPluginCommand> GetCommands() => Impl.GetCommandsList();
    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target) => Impl.GetMenuNodes(target);
#if false

    private sealed class LoggerHandler(Func<IPluginLogger?, ILogger> loggerFactory, Action<ILogger> setLogger, Action clearLogger)
    {
        public static LoggerHandler Create<T>(Action<ILogger> setLogger, Action clearLogger) => new(Loggers.CreateLogger<T>, setLogger, clearLogger);
        public static LoggerHandler Create<T>()  where T : ILoggerHolder => Create<T>(T.SetLogger, () => T.SetLogger(null));

        public void SetLogger(IPluginLogger? logger) => setLogger.Invoke(loggerFactory.Invoke(logger));

        public void ClearLogger() => clearLogger.Invoke();

        public static IEnumerable<LoggerHandler> All => [
            Create<SoundVolumeViewExe>(),
        ];
    }

    private static void SetAllLoggers(IPluginLogger logger)
    {
        foreach (LoggerHandler handler in LoggerHandler.All)
            handler.SetLogger(logger);
    }

    private static void ClearAllLoggers()
    {
        foreach (LoggerHandler handler in LoggerHandler.All)
            handler.ClearLogger();
    }
}

internal interface ILoggerHolder
{
    abstract static void SetLogger(ILogger? logger);
#endif
}