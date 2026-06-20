#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoupixDeck.Models;
using LoupixDeck.Models.Converter;
using LoupixDeck.Services;
using LoupixDeck.Services.Plugins;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace LoupixDeck.ViewModels;

public partial class SettingsViewModel : DialogViewModelBase<DialogResult>
{
    public LoupedeckConfig Config { get; }
    private readonly IDeviceService _deviceService;
    private readonly IPageManager _pageManager;
    private readonly IDialogService _dialogService;
    private readonly IInterceptionService _interceptionService;
    private readonly IPluginReloadService _pluginReload;
    private readonly IPluginManager _pluginManager;

    /// <summary>
    /// All discovered plugins — drives the Plugins settings page. Read live from the
    /// manager (its list is swapped on hot-reload), never cached, so the UI re-reads
    /// the current snapshot after an enable/disable/install/remove.
    /// </summary>
    public IReadOnlyList<LoadedPlugin> Plugins => _pluginManager.Plugins;

    public IAsyncRelayCommand InstallInterceptionCommand => field ??= Relay.Create(InstallInterceptionAsync);
    public IAsyncRelayCommand UninstallInterceptionCommand => field ??= Relay.Create(UninstallInterceptionAsync);

    public ObservableCollection<VibrationPatternItem> VibrationPatterns => VibrationPatternCatalog.All;

    /// <summary>
    /// App-focus page switching is available on Windows and on Linux (X11/XWayland).
    /// Gates the "App Switching" settings page — wider than <see cref="OperatingSystem.IsWindows"/>,
    /// so the editor stays hidden only on macOS / unsupported platforms.
    /// </summary>
    [SupportedOSPlatformGuard("windows")]
    [SupportedOSPlatformGuard("linux")]
    public bool IsAppSwitchingSupported => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

    public bool IsWindows => OperatingSystem.IsWindows();

    public SettingsViewModel(LoupedeckConfig config,
        IDeviceService deviceService,
        IPageManager pageManager,
        IDialogService dialogService,
        IPluginManager pluginManager,
        IPluginReloadService pluginReload,
        IInterceptionService interceptionService)
    {
        Config = config;
        _deviceService = deviceService;
        _pageManager = pageManager;
        _dialogService = dialogService;
        _interceptionService = interceptionService;
        _pluginReload = pluginReload;
        _pluginManager = pluginManager;



        // CollectionChanged can fire from a background thread (the parameterless
        // RelayCommand runs Execute via Task.Run). Marshal the CanExecute refresh
        // back to the UI thread so Avalonia's Button.IsEnabled update is safe.
        _pageManager.TouchButtonPages.CollectionChanged += (_, _) =>
            Dispatcher.UIThread.Post(() =>
            {
                RefreshTouchPageCommands();
                SyncFallbackPageOptions();
            });
        _pageManager.RotaryButtonPages.CollectionChanged += (_, _) =>
            Dispatcher.UIThread.Post(() =>
            {
                RefreshRotaryPageCommands();
                SyncRotaryPageOptions();
            });

        // Side-page lists drive the Remove buttons' CanExecute on side-strip devices.
        if (HasIndependentRotarySides)
        {
            LeftRotaryPages.CollectionChanged += (_, _) =>
                Dispatcher.UIThread.Post(() =>
                    RemoveLeftRotaryPageCommand.NotifyCanExecuteChanged());
            RightRotaryPages.CollectionChanged += (_, _) =>
                Dispatcher.UIThread.Post(() =>
                    RemoveRightRotaryPageCommand.NotifyCanExecuteChanged());
        }

        SyncRotaryPageOptions();
        SyncFallbackPageOptions();

        // Build the editor rows from the persisted rules. Rows and
        // Config.AppPageBindings are kept in sync manually in Add/RemoveAppBinding
        // (the only mutation paths), so no event subscription is needed here.
        foreach (var binding in Config.AppPageBindings)
            AppBindingRows.Add(new AppBindingRow(binding, TouchPages, RotaryPageOptions));

        Config.HapticSteps.CollectionChanged += OnHapticStepsChanged;

        CurrentView = SettingsView.General;

        // Device info call blocks on a serial round-trip — push it off the UI thread.
        _ = Task.Run(RefreshDeviceInfoAsync);

        // Probe the Interception driver state (DLL API) off the UI thread; drives the page.
        if (OperatingSystem.IsWindows())
            _ = Task.Run(RefreshInterceptionStatus);

        Version = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?"}";
    }

    // ───────── General / Device ─────────

    public string DeviceName => _deviceService?.Device?.Type ?? "Device";

    [ObservableProperty]
    public partial string DeviceVersion { get; private set; } = "—";

    [ObservableProperty]
    public partial string DeviceSerial { get; private set; } = "—";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DeviceStatusText))]
    public partial bool DeviceConnected { get; private set; }

    public string DeviceStatusText => DeviceConnected ? "Connected" : "Disconnected";

    private async Task RefreshDeviceInfoAsync()
    {
        var dev = _deviceService?.Device;
        if (dev == null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DeviceConnected = false;
                DeviceVersion = "—";
                DeviceSerial = "—";
            });
            return;
        }

        string version = "—";
        string serialHex = "—";
        var ok = false;
        try
        {
            // Blocks on serial round-trip (GetInfo does Send().GetAwaiter().GetResult()).
            // We are already off the UI thread because the caller used Task.Run.
            var (serial, ver) = dev.GetInfo();
            version = ver ?? "—";
            serialHex = serial != null ? BitConverter.ToString(serial).Replace("-", "") : "—";
            ok = true;
        }
        catch
        {
            // fall through with defaults
        }

        Dispatcher.UIThread.Post(() =>
        {
            DeviceVersion = version;
            DeviceSerial = serialHex;
            DeviceConnected = ok;
        });

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ReconnectDevice()
    {
        await Task.Run(() =>
        {
            try { _deviceService?.ReconnectDevice(); }
            catch { /* ignored */ }
        });
        await Task.Delay(500);
        await Task.Run(RefreshDeviceInfoAsync);
    }

    [RelayCommand]
    private Task AddTouchPage() => _pageManager.AddTouchButtonPage();

    // ───────── Interception (Windows only) ─────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InterceptionStatusText))]
    public partial bool InterceptionDriverInstalled { get; private set; }

    public string InterceptionStatusText => InterceptionDriverInstalled ? "Installed" : "Not installed";

    [ObservableProperty]
    public partial bool InterceptionBusy { get; private set; }

    [ObservableProperty]
    public partial string InterceptionStatusMessage { get; private set; } = string.Empty;

    /// <summary>
    /// The Use-Interception toggle. Maps the tri-state config (null = auto) onto a plain bool:
    /// auto is shown as enabled, so an installed driver is used by default.
    /// </summary>
    public bool InterceptionEnabled
    {
        get => Config.InterceptionEnabled ?? true;
        set
        {
            if ((Config.InterceptionEnabled ?? true) == value) return;
            Config.InterceptionEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Opens the user plugins folder in the OS file manager, creating it first
    /// if missing. This is the per-build config plugins dir
    /// (<c>~/.config/LoupixDeck[/debug]/plugins</c>), where users drop their own
    /// plugins. UseShellExecute=true routes a directory path through Explorer on
    /// Windows / xdg-open on Linux.
    /// </summary>
    [RelayCommand]
    private void OpenPluginsFolder()
    {
        try
        {
            var dir = System.IO.Path.Combine(FileDialogHelper.GetConfigDir(), "plugins");
            System.IO.Directory.CreateDirectory(dir);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            });
        }
        catch { }
    }

    /// <summary>Installs (or updates) a plugin from the given zip and loads it live.</summary>
    public Task<PluginActionResult> InstallPluginFromZipAsync(string zipPath) =>
        _pluginReload.InstallAsync(zipPath);

    /// <summary>Unloads and removes an installed (user) plugin live.</summary>
    public Task<PluginActionResult> RemovePluginAsync(LoadedPlugin plugin) =>
        _pluginReload.RemoveAsync(plugin);

    /// <summary>Loads a plugin live (no restart).</summary>
    public Task<PluginActionResult> EnablePluginAsync(string pluginId) =>
        _pluginReload.EnableAsync(pluginId);

    /// <summary>Unloads a plugin live (no restart).</summary>
    public Task<PluginActionResult> DisablePluginAsync(string pluginId) =>
        _pluginReload.DisableAsync(pluginId);

    private void RefreshInterceptionStatus()
    {
        var installed = _interceptionService.IsDriverInstalled();
        Dispatcher.UIThread.Post(() => InterceptionDriverInstalled = installed);
    }

    private async Task InstallInterceptionAsync()
    {
        InterceptionBusy = true;
        try
        {
            var progress = new Progress<string>(msg =>
                Dispatcher.UIThread.Post(() => InterceptionStatusMessage = msg));
            await _interceptionService.DownloadAndInstallAsync(progress);
        }
        finally
        {
            InterceptionBusy = false;
            await Task.Run(RefreshInterceptionStatus);
        }
    }

    private async Task UninstallInterceptionAsync()
    {
        InterceptionBusy = true;
        try
        {
            var progress = new Progress<string>(msg =>
                Dispatcher.UIThread.Post(() => InterceptionStatusMessage = msg));
            await _interceptionService.UninstallAsync(progress);
        }
        finally
        {
            InterceptionBusy = false;
            await Task.Run(RefreshInterceptionStatus);
        }
    }

    // ───────── Pages ─────────

    public ObservableCollection<TouchButtonPage> TouchPages => _pageManager.TouchButtonPages;
    public ObservableCollection<RotaryButtonPage> RotaryPages => _pageManager.RotaryButtonPages;

    /// <summary>True for devices that page their two dial columns independently (Razer):
    /// the rotary settings show separate left/right page lists instead of one.</summary>
    public bool HasIndependentRotarySides => _pageManager.HasIndependentRotarySides;
    public ObservableCollection<RotaryButtonPage> LeftRotaryPages => _pageManager.GetRotaryPages(RotarySide.Left);
    public ObservableCollection<RotaryButtonPage> RightRotaryPages => _pageManager.GetRotaryPages(RotarySide.Right);

    public ObservableCollection<int> TouchPageIndices
    {
        get
        {
            var c = new ObservableCollection<int>();
            if (TouchPages != null)
                for (var i = 0; i < TouchPages.Count; i++) c.Add(i);
            return c;
        }
    }

    // ───────── App Switching ─────────

    // The rule editor's touch-page selector binds directly to the live TouchPages
    // collection (a stable instance). A regenerated list would make the ComboBox
    // reset its SelectedIndex and — via the TwoWay binding — write that reset back
    // over the user's choice, so the selection appeared to "not save".

    /// <summary>Rotary page labels with a leading "(unchanged)" entry — maps to
    /// <c>AppPageBinding.RotarySelectionIndex</c> (0 = unchanged, n = page n). Kept
    /// as a single stable instance and synced in place (tail add/remove) so bound
    /// ComboBoxes never lose their selection.</summary>
    public ObservableCollection<string> RotaryPageOptions { get; } = new();

    /// <summary>Editor rows bound by the rule list. Kept in sync with
    /// <c>Config.AppPageBindings</c> in <see cref="AddAppBinding"/> /
    /// <see cref="RemoveAppBinding"/>.</summary>
    public ObservableCollection<AppBindingRow> AppBindingRows { get; } = new();

    /// <summary>Touch page labels for the no-match fallback selector, with a leading
    /// "(do nothing)" entry. Stable instance synced in place (tail add/remove).</summary>
    public ObservableCollection<string> FallbackPageOptions { get; } = new();

    private void SyncFallbackPageOptions()
    {
        if (FallbackPageOptions.Count == 0) FallbackPageOptions.Add("(do nothing)");
        var want = (TouchPages?.Count ?? 0) + 1; // +1 for the "(do nothing)" entry
        while (FallbackPageOptions.Count > want) FallbackPageOptions.RemoveAt(FallbackPageOptions.Count - 1);
        while (FallbackPageOptions.Count < want) FallbackPageOptions.Add($"Page {FallbackPageOptions.Count}");
    }

    /// <summary>ComboBox helper for the no-match fallback: 0 = "(do nothing)" (null),
    /// n = touch page n-1. Maps to/from <c>Config.AppSwitchingFallbackTouchPageIndex</c>.</summary>
    public int FallbackSelectionIndex
    {
        get => Config.AppSwitchingFallbackTouchPageIndex is { } idx ? idx + 1 : 0;
        set
        {
            var newValue = value <= 0 ? (int?)null : value - 1;
            if (Config.AppSwitchingFallbackTouchPageIndex == newValue) return;
            Config.AppSwitchingFallbackTouchPageIndex = newValue;
            OnPropertyChanged();
        }
    }

    private void SyncRotaryPageOptions()
    {
        if (RotaryPageOptions.Count == 0) RotaryPageOptions.Add("(unchanged)");
        var want = (RotaryPages?.Count ?? 0) + 1; // +1 for the "(unchanged)" entry
        while (RotaryPageOptions.Count > want) RotaryPageOptions.RemoveAt(RotaryPageOptions.Count - 1);
        while (RotaryPageOptions.Count < want) RotaryPageOptions.Add($"Page {RotaryPageOptions.Count}");
    }

    [RelayCommand]
    private void AddAppBinding()
    {
        var binding = new AppPageBinding();
        Config.AppPageBindings.Add(binding);
        AppBindingRows.Add(new AppBindingRow(binding, TouchPages, RotaryPageOptions));
    }

    private bool CanRemoveAppBindings(AppBindingRow row) => row != null;

    [RelayCommand(CanExecute = nameof(CanRemoveAppBindings))]
    private void RemoveAppBinding(AppBindingRow row)
    {
        Debug.Assert(CanRemoveAppBindings(row));
        if (row == null) return;
        Config.AppPageBindings.Remove(row.Binding);
        AppBindingRows.Remove(row);
    }

    private bool CanRemoveTouchPage(TouchButtonPage? p) => p != null && _pageManager.TouchButtonPages.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveTouchPage))]
    private void RemoveTouchPage(TouchButtonPage page) => _ = RemoveTouchPageAsync(page);
    private async Task RemoveTouchPageAsync(TouchButtonPage page)
    {
        Debug.Assert(CanRemoveTouchPage(page));
        if (page == null || TouchPages.Count <= 1) return;
        var idx = TouchPages.IndexOf(page);
        if (idx < 0) return;
        _pageManager.CurrentTouchPageIndex = idx;
        await _pageManager.DeleteTouchButtonPage();
    }

    private bool CanRemoveRotaryPage(RotaryButtonPage? p) => p != null && _pageManager.RotaryButtonPages.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRemoveRotaryPage))]
    private void RemoveRotaryPage(RotaryButtonPage page)
    {
        Debug.Assert(CanRemoveRotaryPage(page));
        if (page == null || RotaryPages.Count <= 1) return;
        var idx = RotaryPages.IndexOf(page);
        if (idx < 0) return;
        _pageManager.CurrentRotaryPageIndex = idx;
        _pageManager.DeleteRotaryButtonPage();
    }

    private void RemoveSideRotaryPage(RotarySide side, RotaryButtonPage page)
    {
        var pages = _pageManager.GetRotaryPages(side);
        if (page == null || pages.Count <= 1) return;
        var idx = pages.IndexOf(page);
        if (idx < 0) return;
        // Point the side's current index at the page to delete, then delete it.
        _pageManager.ApplyRotaryPage(side, idx);
        _pageManager.DeleteRotaryButtonPage(side);
    }

    private void MovePage<T>(ObservableCollection<T> coll, T page, int delta)
    {
        if (page == null) return;
        var idx = coll.IndexOf(page);
        var target = idx + delta;
        if (idx < 0 || target < 0 || target >= coll.Count) return;
        coll.Move(idx, target);

        // Renumber pages so PageName reflects the new order.
        var counter = 0;
        foreach (var item in coll)
        {
            counter++;
            switch (item)
            {
                case TouchButtonPage tp: tp.Page = counter; break;
                case RotaryButtonPage rp: rp.Page = counter; break;
            }
        }

        // Keep the current-page index pointing at the same page after the move.
        if (typeof(T) == typeof(TouchButtonPage))
        {
            _pageManager.CurrentTouchPageIndex = AdjustCurrentIndex(
                _pageManager.CurrentTouchPageIndex, idx, target);
        }
        else if (typeof(T) == typeof(RotaryButtonPage))
        {
            _pageManager.CurrentRotaryPageIndex = AdjustCurrentIndex(
                _pageManager.CurrentRotaryPageIndex, idx, target);
        }
    }

    private static int AdjustCurrentIndex(int current, int from, int to)
    {
        if (current == from) return to;
        if (from < current && to >= current) return current - 1;
        if (from > current && to <= current) return current + 1;
        return current;
    }

    private void RefreshTouchPageCommands()
    {
        MoveTouchPageUpCommand.NotifyCanExecuteChanged();
        MoveTouchPageDownCommand.NotifyCanExecuteChanged();
        RemoveTouchPageCommand.NotifyCanExecuteChanged();
    }

    private void RefreshRotaryPageCommands()
    {
        MoveRotaryPageUpCommand.NotifyCanExecuteChanged();
        MoveRotaryPageDownCommand.NotifyCanExecuteChanged();
        RemoveRotaryPageCommand.NotifyCanExecuteChanged();
    }

    // ───────── Haptic ─────────

    public const int MaxHapticSteps = 2;

    private void OnHapticStepsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(FirstHapticStep));
        OnPropertyChanged(nameof(SecondHapticStep));
        OnPropertyChanged(nameof(HasSecondHapticStep));
        OnPropertyChanged(nameof(CanAddHapticStep));
    }

    public HapticStep? FirstHapticStep =>
        Config.HapticSteps.Count > 0 ? Config.HapticSteps[0] : null;

    public HapticStep? SecondHapticStep =>
        Config.HapticSteps.Count > 1 ? Config.HapticSteps[1] : null;

    public bool HasSecondHapticStep => Config.HapticSteps.Count > 1;
    public bool CanAddHapticStep => Config.HapticSteps.Count < MaxHapticSteps;

    [RelayCommand]
    private void AddHapticStep()
    {
        if (Config.HapticSteps.Count >= MaxHapticSteps) return;
        Config.HapticSteps.Add(new HapticStep());
    }

    [RelayCommand]
    private void RemoveHapticStep(HapticStep step)
    {
        if (Config.HapticSteps.Count <= 1) return;
        Config.HapticSteps.RemoveAt(Config.HapticSteps.Count - 1);
    }

    // ───────── Theme ─────────

    public bool ThemeIsDark
    {
        get => string.Equals(Config.ThemeVariant, "Dark", StringComparison.OrdinalIgnoreCase);
        set { if (value) ApplyTheme("Dark"); }
    }
    public bool ThemeIsLight
    {
        get => string.Equals(Config.ThemeVariant, "Light", StringComparison.OrdinalIgnoreCase);
        set { if (value) ApplyTheme("Light"); }
    }
    public bool ThemeIsSystem
    {
        get => string.Equals(Config.ThemeVariant, "System", StringComparison.OrdinalIgnoreCase);
        set { if (value) ApplyTheme("System"); }
    }

    private void ApplyTheme(string variant)
    {
        if (Config.ThemeVariant == variant) return;
        Config.ThemeVariant = variant;
        OnPropertyChanged(nameof(ThemeIsDark));
        OnPropertyChanged(nameof(ThemeIsLight));
        OnPropertyChanged(nameof(ThemeIsSystem));

        Application.Current?.RequestedThemeVariant = variant switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
    }

    // ───────── About ─────────

    public string Version { get; }

    // ───────── View navigation ─────────

    [ObservableProperty]
    public partial SettingsView CurrentView { get; set; }

    private bool CanEditWallpaper(TouchButtonPage? p) => p != null;

    [RelayCommand(CanExecute = nameof(CanEditWallpaper))]
    private async Task EditWallpaper(TouchButtonPage page)
    {
        if (page == null) return;
        await _dialogService.ShowDialogAsync<TouchPageWallpaperSettingsViewModel, DialogResult>(
            vm => vm.Initialize(page));
    }

    private bool CanEditPageCommands(object? p) => p is TouchButtonPage or RotaryButtonPage;

    [RelayCommand(CanExecute = nameof(CanEditPageCommands))]
    private async Task EditPageCommands(object page)
    {
        if (page is not TouchButtonPage && page is not RotaryButtonPage) return;
        await _dialogService.ShowDialogAsync<PageCommandsSettingsViewModel, DialogResult>(
            vm => vm.Initialize(page));
    }

    [RelayCommand]
    private void Navigate(SettingsView settingsPage) => CurrentView = settingsPage;

    [RelayCommand]
    private void OpenWebsite()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/RadiatorTwo/LoupixDeck",
                UseShellExecute = true
            });
        }
        catch { }
    }

    private bool CanMoveTouchPageUp(TouchButtonPage? p) => p != null && _pageManager.TouchButtonPages.IndexOf(p) > 0;
    [RelayCommand(CanExecute = nameof(CanMoveTouchPageUp))]
    private void MoveTouchPageUp(TouchButtonPage page) => MovePage(_pageManager.TouchButtonPages, page, -1);

    private bool CanMoveTouchPageDown(TouchButtonPage? p) => p != null && _pageManager.TouchButtonPages.IndexOf(p) < _pageManager.TouchButtonPages.Count - 1;
    [RelayCommand(CanExecute = nameof(CanMoveTouchPageDown))]
    private void MoveTouchPageDown(TouchButtonPage page) => MovePage(_pageManager.TouchButtonPages, page, +1);

    [RelayCommand]
    private void AddRotaryPage() => _pageManager.AddRotaryButtonPage();

    private bool CanMoveRotaryPageUp(RotaryButtonPage? p) => p != null && _pageManager.RotaryButtonPages.IndexOf(p) > 0;
    [RelayCommand(CanExecute = nameof(CanMoveRotaryPageUp))]
    private void MoveRotaryPageUp(RotaryButtonPage p) => MovePage(_pageManager.RotaryButtonPages, p, -1);

    private bool CanMoveRotaryPageDown(RotaryButtonPage? p) => p != null && _pageManager.RotaryButtonPages.IndexOf(p) < _pageManager.RotaryButtonPages.Count - 1;
    [RelayCommand(CanExecute = nameof(CanMoveRotaryPageDown))]
    private void MoveRotaryPageDown(RotaryButtonPage p) => MovePage(_pageManager.RotaryButtonPages, p, +1);

    [RelayCommand]
    private void AddLeftRotaryPage() => _pageManager.AddRotaryButtonPage(RotarySide.Left);

    private bool CanRemoveLeftRotaryPage(RotaryButtonPage? p) => p != null && LeftRotaryPages.Count > 1;
    [RelayCommand(CanExecute = nameof(CanRemoveLeftRotaryPage))]
    private void RemoveLeftRotaryPage(RotaryButtonPage p) => RemoveSideRotaryPage(RotarySide.Left, p);

    [RelayCommand]
    private void AddRightRotaryPage() => _pageManager.AddRotaryButtonPage(RotarySide.Right);

    private bool CanRemoveRightRotaryPage(RotaryButtonPage? p) => p != null && RightRotaryPages.Count > 1;
    [RelayCommand(CanExecute = nameof(CanRemoveRightRotaryPage))]
    private void RemoveRightRotaryPage(RotaryButtonPage p) => RemoveSideRotaryPage(RotarySide.Right, p);
}
