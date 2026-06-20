#nullable enable
using CommunityToolkit.Mvvm.Input;

namespace LoupixDeck.Utils;

public static class Relay
{
    public static IRelayCommand Create(Action execute) => new RelayCommand(execute);
    public static IRelayCommand Create(Action execute, Func<bool> canExecute) => new RelayCommand(execute, canExecute);
    public static IRelayCommand<T> Create<T>(Action<T> execute) => new RelayCommand<T>(execute);
    public static IRelayCommand<T> Create<T>(Action<T> execute, Predicate<T> canExecute) => new RelayCommand<T>(execute, canExecute);
    public static IAsyncRelayCommand Create(Func<Task> execute) => new AsyncRelayCommand(execute);
    public static IAsyncRelayCommand<T> Create<T>(Func<T, Task> execute) => new AsyncRelayCommand<T>(execute);
    public static IAsyncRelayCommand<T> Create<T>(Func<T, Task> execute, Predicate<T> canExecute) => new AsyncRelayCommand<T>(execute, canExecute);

    public static IRelayCommand Create(ref IRelayCommand field, Action execute) => field ??= Create(execute);
    public static IRelayCommand Create(ref IRelayCommand field, Action execute, Func<bool> canExecute) => field ??= Create(execute, canExecute);
    public static IRelayCommand<T> Create<T>(ref IRelayCommand<T> field, Action<T> execute) => field ??= Create(execute);
    public static IRelayCommand<T> Create<T>(ref IRelayCommand<T> field, Action<T> execute, Predicate<T> canExecute) => field ??= Create(execute, canExecute);
    public static IAsyncRelayCommand Create(ref IAsyncRelayCommand field, Func<Task> execute) => field ??= Create(execute);
    public static IAsyncRelayCommand<T> Create<T>(ref IAsyncRelayCommand<T> field, Func<T, Task> execute) => field ??= Create(execute);
    public static IAsyncRelayCommand<T> Create<T>(ref IAsyncRelayCommand<T> field, Func<T, Task> execute, Predicate<T> canExecute) => field ??= Create(execute, canExecute);
}