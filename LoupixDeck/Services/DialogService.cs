using Avalonia.Controls;
using LoupixDeck.Models;
using LoupixDeck.Utils;
using LoupixDeck.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics;

namespace LoupixDeck.Services;

public interface IDialogService
{
    Task<DialogResult> ShowDialogAsync<TViewModel, TResult>(Action<TViewModel>? initializer = null)
        where TViewModel : IDialogViewModel;

    void Register<TViewModel, TWindow>()
        where TWindow : Window;
}

public class DialogService(IServiceProvider serviceProvider) : IDialogService
{
    private ImmutableDictionary<Type, Type> _viewModelToWindowMap = ImmutableDictionary<Type, Type>.Empty;

    public void Register<TViewModel, TWindow>()
        where TWindow : Window
    {
        ImmutableInterlocked.Update(ref _viewModelToWindowMap, static map => map.SetItem(typeof(TViewModel), typeof(TWindow)));
    }

    public async Task<DialogResult> ShowDialogAsync<TViewModel, TResult>(Action<TViewModel>? initializer = null)
        where TViewModel : IDialogViewModel
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        initializer?.Invoke(viewModel);

        if (!_viewModelToWindowMap.TryGetValue(typeof(TViewModel), out var windowType))
            throw new InvalidOperationException($"No window registered for {typeof(TViewModel).Name}");

        // Prefer a (ViewModel) ctor so the window can set DataContext *before*
        // InitializeComponent runs — that prevents spurious binding warnings on
        // $parent[Window].DataContext.X during the first XAML evaluation pass.
        Window window;
        var ctorWithVm = windowType.GetConstructor(new[] { typeof(TViewModel) });
        if (ctorWithVm != null)
        {
            window = (Window)ctorWithVm.Invoke(new object[] { viewModel })!;
        }
        else
        {
            window = (Window)Activator.CreateInstance(windowType)!;
            window.DataContext = viewModel;
        }
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        if (viewModel is IAsyncInitViewModel asyncInit)
        {
            // Kick off async init without blocking. The sync prefix of
            // InitializeAsync (before its first await) runs immediately so
            // bindings that read initial collection references stay valid.
            _ = asyncInit.InitializeAsync();
        }

        Window? mainWindow = WindowHelper.GetMainWindow();
        Debug.Assert(mainWindow is not null, "Main window should be available when showing a dialog");
        await window.ShowDialog(mainWindow);
        return await viewModel.DialogResult.Task;
    }
}