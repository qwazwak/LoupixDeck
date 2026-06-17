using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LoupixDeck.LoupedeckDevice;
using Newtonsoft.Json;

namespace LoupixDeck.Models;

public partial class SimpleButton : LoupedeckButton
{
    public Constants.ButtonType Id { get; set; }

    [ObservableProperty]
    //[NotifyPropertyChangedFor(nameof(TextColor))]
    public partial Color ButtonColor { get; set; }
    partial void OnButtonColorChanged(Color value) => Refresh();

    [JsonIgnore]
    [ObservableProperty]
    public partial Bitmap RenderedImage { get; set; }
}