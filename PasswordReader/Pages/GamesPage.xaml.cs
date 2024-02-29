using System.Linq.Expressions;

namespace PasswordReader.Pages;

public partial class GamesPage : ContentPage
{
    public GamesPage()
    {
        InitializeComponent();
    }

    private void OnArkanoidClicked(object sender, EventArgs e) => GoTo("//arkanoid");

    private void OnBackgammonClicked(object sender, EventArgs e) => GoTo("//backgammon");

    private void OnChessClicked(object sender, EventArgs e) => GoTo("//chess");

    private void OnTetrisClicked(object sender, EventArgs e) => GoTo("//tetris");

    private async void GoTo(string page)
    {
        try
        {
            await Shell.Current.GoToAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}