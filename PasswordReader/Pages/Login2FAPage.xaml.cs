using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

public partial class Login2FAPage : ContentPage
{
    private ContextViewModel _model;

	public Login2FAPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    private async void Confirm_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.ContextService.Login2FAAsync(_model.SecurityCode);
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
        if (string.IsNullOrEmpty(_model.EncryptionKey))
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