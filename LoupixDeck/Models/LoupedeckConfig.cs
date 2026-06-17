using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace LoupixDeck.Models;

/// <summary>
/// This data model holds all configuration settings,
/// which are loaded and saved via JSON.
/// </summary>
[ObservableObject]
public partial class LoupedeckConfig
{
    public LoupedeckConfig()
    {
        // Newtonsoft populates the field-initialized collections in place (no
        // ObjectCreationHandling.Replace), so the property setters never fire on
        // load — subscribe here to keep the page-count labels in sync.
        RotaryButtonPages.CollectionChanged += OnRotaryPagesChanged;
        LeftRotaryButtonPages.CollectionChanged += OnLeftRotaryPagesChanged;
        RightRotaryButtonPages.CollectionChanged += OnRightRotaryPagesChanged;
        TouchButtonPages.CollectionChanged += OnTouchPagesChanged;
    }

    /// <summary>
    /// Schema version of the persisted config. <see cref="ConfigService"/> runs
    /// the migration chain for older versions (see <c>Services/Migrations</c>).
    /// v3 introduced the plugin system: the integration-specific fields were
    /// removed and the per-integration enable flags became <see cref="EnabledPlugins"/>.
    /// v4 split the single <see cref="RotaryButtonPages"/> list into independent
    /// <see cref="LeftRotaryButtonPages"/> / <see cref="RightRotaryButtonPages"/> sets
    /// for devices with side strips (Razer); see <c>RotaryPageSideSplitMigrator</c>.
    /// v5 moved page wallpapers from inline Base64 into the asset folder (relative path
    /// + scaling parameters on each page); see <c>WallpaperAssetMigrator</c>.
    /// v6 nested the flat main-wallpaper fields into a <c>MainWallpaper</c> slot and added
    /// optional left/right side-display wallpapers; see <c>WallpaperSlotMigrator</c>.
    /// </summary>
    public const int CurrentVersion = 6;

    public int Version { get; set; } = CurrentVersion;

    public string DevicePort { get; set; }
    public int DeviceBaudrate { get; set; }

    /// <summary>USB vendor ID of the device this config belongs to (hex, e.g. "2ec2").</summary>
    public string DeviceVid { get; set; }

    /// <summary>USB product ID of the device this config belongs to (hex, e.g. "0006").</summary>
    public string DevicePid { get; set; }

    /// <summary>
    /// Normalized USB iSerialNumber of the physical device this config belongs to
    /// (platform-uniform; see <c>SerialNormalizer</c>). Null for devices without a
    /// real serial. Used to scope the config file and to re-detect the right port
    /// when two identical devices are present. Additive — absent in older configs.
    /// </summary>
    public string DeviceSerial { get; set; }

    public int StartupTouchPageIndex { get; set; }
    public string ThemeVariant { get; set; } = "Dark";

    public CloseButtonBehavior CloseButtonBehavior { get; set; } = CloseButtonBehavior.MinimizeToTray;
    public bool StartMinimizedToTray { get; set; }

    // Windows only: route keyboard and mouse macros through the Interception kernel driver
    // instead of SendInput, so injected input reaches raw-input apps (games / anti-cheat).
    // null = "auto" (active when the driver is installed); false = explicitly off.
    // Missing in older config.json simply stays null → auto behaviour (backward compatible).
    [ObservableProperty]
    public partial bool? InterceptionEnabled { get; set; }

    // Visual flash overlay on touch press — useful especially on the Razer
    // (no LED ring on touch buttons) so the user gets visible feedback.
    [ObservableProperty]
    public partial bool TouchFeedbackEnabled { get; set; }
    [ObservableProperty]
    public partial Avalonia.Media.Color TouchFeedbackColor { get; set; } = Avalonia.Media.Colors.White;

    public double TouchFeedbackOpacity
    {
        get;
        set { if (Math.Abs(field - value) < 0.0001) return; field = value; OnPropertyChanged(); }
    } = 0.5;

    // While a finger is down, ignore further TOUCH_START events until TOUCH_END.
    // Defends against the device emitting duplicate TOUCH_START at button
    // boundaries or when the finger slides across slots.
    [ObservableProperty]
    public partial bool TouchSlidingPreventionEnabled { get; set; } = true;

    public SimpleButton[] SimpleButtons { get; set; }

    public ObservableCollection<RotaryButtonPage> RotaryButtonPages
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field?.CollectionChanged -= OnRotaryPagesChanged;
            field = value;
            field?.CollectionChanged += OnRotaryPagesChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RotaryPageLabel));
        }
    } = [];

    private void OnRotaryPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(RotaryPageLabel));

    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentRotaryButtonPage), nameof(RotaryPageLabel))]
    public partial int CurrentRotaryPageIndex { get; set; } = -1;

    [JsonIgnore]
    public RotaryButtonPage CurrentRotaryButtonPage =>
        (RotaryButtonPages != null &&
         CurrentRotaryPageIndex >= 0 &&
         CurrentRotaryPageIndex < RotaryButtonPages.Count)
            ? RotaryButtonPages[CurrentRotaryPageIndex]
            : null;

    /// <summary>"current / total" label for the rotary pager (1-based).</summary>
    [JsonIgnore]
    public string RotaryPageLabel =>
        RotaryButtonPages is { Count: > 0 }
            ? $"{Math.Clamp(CurrentRotaryPageIndex + 1, 1, RotaryButtonPages.Count)} / {RotaryButtonPages.Count}"
            : "0 / 0";

    // --- Independent left/right rotary pages (devices with side strips) -------
    // Devices with side strips (Razer Stream Controller) page each dial column on
    // its own: LeftRotaryButtonPages / RightRotaryButtonPages each hold that side's
    // knobs (3 on the Razer, re-indexed 0-based per side). Devices without side
    // strips (Live S) leave these empty and keep using RotaryButtonPages (Both).

    public ObservableCollection<RotaryButtonPage> LeftRotaryButtonPages
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field?.CollectionChanged -= OnLeftRotaryPagesChanged;
            field = value;
            field?.CollectionChanged += OnLeftRotaryPagesChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LeftRotaryPageLabel));
        }
    } = [];

    private void OnLeftRotaryPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(LeftRotaryPageLabel));

    public ObservableCollection<RotaryButtonPage> RightRotaryButtonPages
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field?.CollectionChanged -= OnRightRotaryPagesChanged;
            field = value;
            field?.CollectionChanged += OnRightRotaryPagesChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RightRotaryPageLabel));
        }
    } = [];

    private void OnRightRotaryPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(RightRotaryPageLabel));

    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentLeftRotaryButtonPage), nameof(LeftRotaryPageLabel))]
    public partial int CurrentLeftRotaryPageIndex { get; set; } = -1;

    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentRightRotaryButtonPage), nameof(RightRotaryPageLabel))]
    public partial int CurrentRightRotaryPageIndex { get; set; } = -1;

    [JsonIgnore]
    public RotaryButtonPage CurrentLeftRotaryButtonPage =>
        (LeftRotaryButtonPages != null &&
         CurrentLeftRotaryPageIndex >= 0 &&
         CurrentLeftRotaryPageIndex < LeftRotaryButtonPages.Count)
            ? LeftRotaryButtonPages[CurrentLeftRotaryPageIndex]
            : null;

    [JsonIgnore]
    public RotaryButtonPage CurrentRightRotaryButtonPage =>
        (RightRotaryButtonPages != null &&
         CurrentRightRotaryPageIndex >= 0 &&
         CurrentRightRotaryPageIndex < RightRotaryButtonPages.Count)
            ? RightRotaryButtonPages[CurrentRightRotaryPageIndex]
            : null;

    /// <summary>"current / total" label for the left rotary pager (1-based).</summary>
    [JsonIgnore]
    public string LeftRotaryPageLabel => FormatLabel(LeftRotaryButtonPages, CurrentLeftRotaryPageIndex);

    /// <summary>"current / total" label for the right rotary pager (1-based).</summary>
    [JsonIgnore]
    public string RightRotaryPageLabel => FormatLabel(RightRotaryButtonPages, CurrentRightRotaryPageIndex);

    // Strip rendering mode is per rotary page (see RotaryButtonPage.StripMode), not
    // global per side — each page on a column can independently be Segmented/FreeDraw.

    public ObservableCollection<TouchButtonPage> TouchButtonPages
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field?.CollectionChanged -= OnTouchPagesChanged;
            field = value;
            field?.CollectionChanged += OnTouchPagesChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TouchPageLabel));
        }
    } = [];

    private void OnTouchPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        => OnPropertyChanged(nameof(TouchPageLabel));

    [JsonIgnore]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentTouchButtonPage), nameof(TouchPageLabel))]
    public partial int CurrentTouchPageIndex { get; set; } = -1;

    [JsonIgnore]
    public TouchButtonPage CurrentTouchButtonPage =>
        CurrentTouchPageIndex >= 0 && CurrentTouchPageIndex < TouchButtonPages?.Count
                ? TouchButtonPages[CurrentTouchPageIndex] : null;

    /// <summary>"current / total" label for the touch pager (1-based).</summary>
    [JsonIgnore]
    public string TouchPageLabel => FormatLabel(TouchButtonPages, CurrentTouchPageIndex);

    [ObservableProperty]
    public partial int Brightness { get; set; } = 100;

    // Briefly draws the page name on touch button 0 after switching pages.
    // Opt-in: many users find the 2s overlay distracting and prefer to keep
    // their layout visible.
    [ObservableProperty]
    public partial bool ShowPageNameOverlayEnabled { get; set; }

    [ObservableProperty]
    public partial bool HapticEnabled { get; set; }

    /// <summary>
    /// Ids of plugins the user has enabled. The v2→v3 migration seeds this from
    /// the former per-integration enable flags (see <c>PluginConfigMigrator</c>).
    /// </summary>
    public List<string> EnabledPlugins { get; set; } = [];

    // ObjectCreationHandling.Replace: Newtonsoft otherwise reuses the default
    // collection and appends deserialized items to it — so each save+load round
    // would duplicate every step.
    [Newtonsoft.Json.JsonProperty(ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace)]
    public ObservableCollection<HapticStep> HapticSteps { get; set; } = [new HapticStep()];

    // --- App-focus page switching (Feature 2) ---------------------------------
    // Master toggle for the foreground-window → page mapping.
    [ObservableProperty]
    public partial bool AppSwitchingEnabled { get; set; }

    // Ordered rule list — first match wins. ObjectCreationHandling.Replace for the
    // same reason as HapticSteps (avoid Newtonsoft appending to the default instance).
    [Newtonsoft.Json.JsonProperty(ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace)]
    public ObservableCollection<AppPageBinding> AppPageBindings { get; set; } = [];

    // Touch page to switch to when no rule matches. null = do nothing on no-match.
    public int? AppSwitchingFallbackTouchPageIndex { get; set; }

    private static string FormatLabel(System.Collections.ICollection? pagesCount, int index) => FormatLabel(pagesCount?.Count, index);
    private static string FormatLabel(int? pagesCount, int index) =>
        pagesCount is > 0
            ? $"{Math.Clamp(index + 1, 1, pagesCount.Value)} / {pagesCount.Value}"
            : "0 / 0";
}