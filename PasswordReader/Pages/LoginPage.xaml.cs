using PasswordReader.Services;
using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

public partial class LoginPage : ContentPage
{
    private ContextViewModel _model;

	public LoginPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    private async void LoginToken_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.ContextService.LoginWithTokenAsync();
            await UpdateModel();
            await GotoNextPage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Login_Clicked(object sender, EventArgs e)
	{
		try
		{
            await App.ContextService.LoginAsync(_model.Username, _model.Password);
            await UpdateModel();
            await GotoNextPage();
        }
        catch (Exception ex)
		{
			await DisplayAlert("Fehler", ex.Message, "OK");
		}
    }

    private async Task UpdateModel()
    {
        _model.Password = "";
        _model.SecurityCode = "";
        _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
        _model.IsLoggedIn = App.ContextService.IsLoggedIn();
        _model.Requires2FA = App.ContextService.Requires2FA();
        _model.HasLoginToken = await App.ContextService.HasLoginTokenAsync();
        _model.HasPasswordItems = App.ContextService.HasPasswordItems();
    }

    private async Task GotoNextPage()
    {
        string page;
        if (App.ContextService.Requires2FA())
        {
            page = "//login2fa";
        }
        else if (string.IsNullOrEmpty(_model.EncryptionKey))
        {
            page = "//encryptionkey";
        }
        else
        {
            page = "//passwordlist";
        }
        await Shell.Current.GoToAsync(page);
    }

}