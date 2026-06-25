using LoupixDeck.PluginSdk;

namespace QPlug;

internal static class ButtonTargetsExtensions
{
    extension(ButtonTargets targets)
    {
        public bool HasAnyButton() => (targets & (ButtonTargets.TouchButton | ButtonTargets.SimpleButton)) is not ButtonTargets.None;
    }
}