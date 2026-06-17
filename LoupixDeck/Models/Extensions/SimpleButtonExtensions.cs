using LoupixDeck.LoupedeckDevice;

namespace LoupixDeck.Models.Extensions;

public static class SimpleButtonExtensions
{
    public static SimpleButton FindById(this SimpleButton[]? buttons, Constants.ButtonType id)
    {
        if (buttons is null)
            return null;
        else
            return Array.Find(buttons, button => button.Id == id);
    }
}