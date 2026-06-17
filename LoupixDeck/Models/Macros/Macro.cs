using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LoupixDeck.Models.Macros;

/// <summary>
/// A named, user-defined macro: an ordered list of steps executed sequentially
/// by <c>System.Macro(Name)</c>. Persisted in macros.json (see <see cref="MacroSettings"/>).
/// </summary>
[ObservableObject]
public partial class Macro
{

    /// <summary>
    /// Unique macro name. Must not contain '(' ')' ',' or '&amp;' — those would break
    /// the command parser when the macro is invoked as System.Macro(Name).
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    public ObservableCollection<MacroStep> Steps { get; set; } = [];
}
