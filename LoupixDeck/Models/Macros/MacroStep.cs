using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace LoupixDeck.Models.Macros;

/// <summary>
/// Base class of a single macro step. Concrete steps carry the persisted data;
/// the <see cref="Icon"/>/<see cref="TypeText"/>/<see cref="ValueText"/> properties
/// are UI projections for the editor's step panels and are never serialized.
/// Serialization is handled by <see cref="Converter.MacroStepJsonConverter"/>, which
/// writes the <see cref="StepType"/> discriminator.
/// </summary>
[ObservableObject]
public abstract partial class MacroStep
{
    /// <summary>Discriminator identifying the concrete step type.</summary>
    [JsonIgnore]
    public abstract MacroStepType StepType { get; }

    /// <summary>MDI font glyph shown in the step panel (use with the MdiFont resource).</summary>
    [JsonIgnore]
    public abstract string Icon { get; }

    /// <summary>Human-readable step type label.</summary>
    [JsonIgnore]
    public abstract string TypeText { get; }

    /// <summary>Short summary of the step's value (text, key combo, delay, ...).</summary>
    [JsonIgnore]
    public abstract string ValueText { get; }

    /// <summary>True while the step panel's inline editor is expanded (editor UI state only).</summary>
    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial bool IsEditing { get; set; }

    /// <summary>True while the step panel is being dragged for reordering (editor UI state only).</summary>
    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial bool IsDragging { get; set; }

    /// <summary>Renders an MDI codepoint as a glyph string.</summary>
    protected static string Glyph(int codepoint) => char.ConvertFromUtf32(codepoint);
}
