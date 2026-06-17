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
public class KeyCombinationStep : MacroStep
{
    /// <summary>Key names joined with '+', same syntax as System.KeyCombination.</summary>
    public string Keys
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyCombination;
    public override string Icon => Glyph(0xF0317); // mdi-keyboard-variant
    public override string TypeText => "Key Combination";
    public override string ValueText => Keys ?? string.Empty;
}

/// <summary>Waits for a fixed amount of time before the next step.</summary>
public class DelayStep : MacroStep
{
    public int Milliseconds
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = 100;

    public override MacroStepType StepType => MacroStepType.Delay;
    public override string Icon => Glyph(0xF051F); // mdi-timer-sand
    public override string TypeText => "Delay";
    public override string ValueText => $"{Milliseconds} ms";
}

/// <summary>Presses (and holds) a single key without releasing it.</summary>
public class KeyDownStep : MacroStep
{
    public string Key
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyDown;
    public override string Icon => Glyph(0xF013C); // mdi-chevron-double-down
    public override string TypeText => "Key Down";
    public override string ValueText => Key ?? string.Empty;
}

/// <summary>Releases a key previously held down by a <see cref="KeyDownStep"/>.</summary>
public class KeyUpStep : MacroStep
{
    public string Key
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = string.Empty;

    public override MacroStepType StepType => MacroStepType.KeyUp;
    public override string Icon => Glyph(0xF013F); // mdi-chevron-double-up
    public override string TypeText => "Key Up";
    public override string ValueText => Key ?? string.Empty;
}

/// <summary>Performs a mouse action (click, button down/up, move, scroll).</summary>
public class MouseStep : MacroStep
{
    /// <summary>All selectable actions/buttons — bound by the editor's ComboBoxes.</summary>
    public static MouseStepAction[] AllActions { get; } = Enum.GetValues<MouseStepAction>();

    public static MouseButton[] AllButtons { get; } = Enum.GetValues<MouseButton>();

    public MouseStepAction Action
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
            OnPropertyChanged(nameof(ShowsButton));
            OnPropertyChanged(nameof(ShowsCoordinates));
            OnPropertyChanged(nameof(ShowsAmount));
            OnPropertyChanged(nameof(ShowsAbsoluteHint));
        }
    } = MouseStepAction.Click;

    /// <summary>Editor visibility helpers — which fields apply to the selected action.</summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsButton => Action is MouseStepAction.Click or MouseStepAction.Down or MouseStepAction.Up;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsCoordinates => Action is MouseStepAction.MoveRelative or MouseStepAction.MoveAbsolute;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsAmount => Action == MouseStepAction.Scroll;

    [Newtonsoft.Json.JsonIgnore]
    public bool ShowsAbsoluteHint => Action == MouseStepAction.MoveAbsolute;

    public MouseButton Button
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = MouseButton.Left;

    /// <summary>X coordinate: relative delta for MoveRelative, screen pixel for MoveAbsolute.</summary>
    public int X
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    }

    /// <summary>Y coordinate: relative delta for MoveRelative, screen pixel for MoveAbsolute.</summary>
    public int Y
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    }

    /// <summary>Scroll amount in wheel detents (positive = up, negative = down).</summary>
    public int Amount
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = 1;

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
public class CommandStep : MacroStep
{
    public string CommandString
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnValueChanged();
        }
    } = string.Empty;

    public override MacroStepType StepType => MacroStepType.Command;
    public override string Icon => Glyph(0xF018D); // mdi-console
    public override string TypeText => "Command";
    public override string ValueText => CommandString ?? string.Empty;
}
