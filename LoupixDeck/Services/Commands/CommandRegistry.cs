using LoupixDeck.PluginSdk;
using System.Collections.Immutable;

namespace LoupixDeck.Services.Commands;

/// <inheritdoc cref="ICommandRegistry"/>
public class CommandRegistry(IEnumerable<ICommandProvider> providers) : ICommandRegistry
{
    // Immutable, copy-on-write map. Initialize() builds a fresh dictionary and
    // publishes it via a single reference swap, so a runtime rebuild
    // (plugin hot-reload) can never tear a read on a device input thread —
    // readers take a local copy of the reference and an in-flight Execute keeps
    // running against the snapshot it already captured.
    private volatile ImmutableDictionary<string, RegisteredCommand> _commands = ImmutableDictionary<string, RegisteredCommand>.Empty.WithComparers(StringComparer.Ordinal);

    public void Initialize() => _commands = CreateInitial(providers); // atomic publish

    private static ImmutableDictionary<string, RegisteredCommand> CreateInitial(IEnumerable<ICommandProvider> providers)
    {
        static IList<RegisteredCommand> EnumerateFromProvider(ICommandProvider provider)
        {
            try
            {
                return provider.GetCommands().ToList();
            }
            catch (Exception ex)
            {
                // A faulty provider (e.g. a misbehaving plugin) must not take
                // down the whole registry.
                Console.WriteLine($"CommandRegistry: provider '{provider.GetType().Name}' failed: {ex.Message}");
                return Array.Empty<RegisteredCommand>();
            }
        }

        return providers.SelectMany(EnumerateFromProvider)
               .Where(static c => !string.IsNullOrEmpty(c?.CommandName))
               .ToImmutableDictionary(static c => c.CommandName, StringComparer.Ordinal);
    }

    public bool Contains(string commandName) => !string.IsNullOrEmpty(commandName) && _commands.ContainsKey(commandName);

    public RegisteredCommand Get(string commandName)
    {
        var map = _commands; // local copy of the reference — never torn
        if (commandName != null && map.TryGetValue(commandName, out var command))
            return command;

        return null;
    }

    public IEnumerable<RegisteredCommand> GetAll() => _commands.Values;

    public async Task Execute(string commandName, string[] parameters, ButtonTargets target, int? sourceIndex = null)
    {
        var command = Get(commandName);
        if (command == null)
        {
            Console.WriteLine($"Command '{commandName}' not found.");
            return;
        }

        await command.Execute(parameters ?? Array.Empty<string>(), target, sourceIndex);
    }
}
