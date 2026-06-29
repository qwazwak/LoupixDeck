#if false
using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;

namespace QCommon;

public abstract class PluginSettingsPage
{

    public abstract ImmutableList<PluginSettingDescriptor> SettingsSchema { get; }

    // Optional
    public virtual ImmutableList<PluginSettingAction> SettingsActions => ImmutableList<PluginSettingAction>.Empty;

    public virtual void OnSettingsSaved() { }
}

internal sealed class PluginSettingsPageComposite(IEnumerable<PluginSettingsPage> contributors)
{
    private static ImmutableArray<T> CreateFromContributors<T>(IEnumerable<PluginSettingsPage> contributors, Func<PluginSettingsPage, ImmutableList<T>> selector)
    {
        contributors.SelectMany(selector).ToImmutableArray()
    }
    public ImmutableArray<PluginSettingDescriptor> SettingsSchema = { get; } 

    // Optional
    public ImmutableArray<PluginSettingAction> SettingsActions => ImmutableArray<PluginSettingAction>.Empty;

    public void OnSettingsSaved() { }
}
#endif