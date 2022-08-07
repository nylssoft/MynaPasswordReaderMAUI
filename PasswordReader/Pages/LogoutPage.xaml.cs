using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

public partial class LogoutPage : ContentPage
{
    private ContextViewModel _model;

    public LogoutPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
    }

    protected async override void OnAppearing()
	{
        base.OnAppearing();
        try
        {
			App.ContextService.Logout();
            _model.Username = "";
            _model.Password = "";
            _model.SecurityCode = "";
            _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            _model.IsLoggedIn = App.ContextService.IsLoggedIn();
            _model.Requires2FA = App.ContextService.Requires2FA();
            _model.HasLoginToken = await App.ContextService.HasLoginTokenAsync();
            _model.HasPasswordItems = App.ContextService.HasPasswordItems();
            _model.PasswordItems = null;
            _model.SelectedPasswordItem = null;
        }
        catch (Exception ex)
		{
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
	}
}