using CommunityToolkit.Mvvm.ComponentModel;

namespace LoupixDeck.Models.Macros;

/// <summary>Types a text string via the virtual keyboard.</summary>
public partial class TextStep : MacroStep
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial string Text { get; set; } = string.Empty;

    public override MacroStepType StepType => MacroStepType.Text;
    public override string Icon => Glyph(0xF030C); // mdi-keyboard
    public override string TypeText => "Type Text";
    public override string ValueText => Text ?? string.Empty;
}

/// <summary>Presses a key combination, e.g. "Ctrl+Shift+Esc".</summary>
public partial class KeyCombinationStep : MacroStep
{
    /// <summary>Key names joined with '+', same syntax as System.KeyCombination.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial string Keys { get; set; } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyCombination;
    public override string Icon => Glyph(0xF0317); // mdi-keyboard-variant
    public override string TypeText => "Key Combination";
    public override string ValueText => Keys ?? string.Empty;
}

/// <summary>Waits for a fixed amount of time before the next step.</summary>
public partial class DelayStep : MacroStep
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial int Milliseconds { get; set; } = 100;

    public override MacroStepType StepType => MacroStepType.Delay;
    public override string Icon => Glyph(0xF051F); // mdi-timer-sand
    public override string TypeText => "Delay";
    public override string ValueText => $"{Milliseconds} ms";
}

/// <summary>Presses (and holds) a single key without releasing it.</summary>
public partial class KeyDownStep : MacroStep
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial string Key { get; set; } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyDown;
    public override string Icon => Glyph(0xF013C); // mdi-chevron-double-down
    public override string TypeText => "Key Down";
    public override string ValueText => Key ?? string.Empty;
}

/// <summary>Releases a key previously held down by a <see cref="KeyDownStep"/>.</summary>
public partial class KeyUpStep : MacroStep
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial string Key { get; set; } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyUp;
    public override string Icon => Glyph(0xF013F); // mdi-chevron-double-up
    public override string TypeText => "Key Up";
    public override string ValueText => Key ?? string.Empty;
}

/// <summary>Performs a mouse action (click, button down/up, move, scroll).</summary>
public partial class MouseStep : MacroStep
{
    /// <summary>All selectable actions/buttons — bound by the editor's ComboBoxes.</summary>
    public static MouseStepAction[] AllActions { get; } = Enum.GetValues<MouseStepAction>();

    public static MouseButton[] AllButtons { get; } = Enum.GetValues<MouseButton>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText), nameof(ShowsButton), nameof(ShowsCoordinates), nameof(ShowsAmount), nameof(ShowsAbsoluteHint))]
    public partial MouseStepAction Action { get; set; } = MouseStepAction.Click;

    /// <summary>Editor visibility helpers — which fields apply to the selected action.</summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsButton => Action is MouseStepAction.Click or MouseStepAction.Down or MouseStepAction.Up;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsCoordinates => Action is MouseStepAction.MoveRelative or MouseStepAction.MoveAbsolute;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsAmount => Action == MouseStepAction.Scroll;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsAbsoluteHint => Action == MouseStepAction.MoveAbsolute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial MouseButton Button { get; set; } = MouseButton.Left;

    /// <summary>X coordinate: relative delta for MoveRelative, screen pixel for MoveAbsolute.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial int X { get; set; }

    /// <summary>Y coordinate: relative delta for MoveRelative, screen pixel for MoveAbsolute.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial int Y { get; set; }

    /// <summary>Scroll amount in wheel detents (positive = up, negative = down).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial int Amount { get; set; } = 1;

    public override MacroStepType StepType => MacroStepType.Mouse;
    public override string Icon => Glyph(0xF037D); // mdi-mouse
    public override string TypeText => "Mouse";

    public override string ValueText => Action switch
    {
        MouseStepAction.Click => $"{Button} Click",
        MouseStepAction.Down => $"{Button} Down",
        MouseStepAction.Up => $"{Button} Up",
        MouseStepAction.MoveRelative => $"Move by {X}, {Y}",
        MouseStepAction.MoveAbsolute => $"Move to {X}, {Y}",
        MouseStepAction.Scroll => $"Scroll {Amount}",
        _ => string.Empty
    };
}

/// <summary>Runs an arbitrary LoupixDeck command string or shell command.</summary>
public partial class CommandStep : MacroStep
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    public partial string CommandString { get; set; } = string.Empty;

    public override MacroStepType StepType => MacroStepType.Command;
    public override string Icon => Glyph(0xF018D); // mdi-console
    public override string TypeText => "Command";
    public override string ValueText => CommandString ?? string.Empty;
}
