using PasswordReader.Pages;
using PasswordReader.ViewModels;

namespace PasswordReader;

public partial class AppShell : Shell
{
    private ContextViewModel _model;
    private bool _init;

    public AppShell()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
        Routing.RegisterRoute("passworditem", typeof(PasswordItemPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_init)
        {
            try
            {
                _init = true;
                _model.Password = "";
                _model.SecurityCode = "";
                _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
                _model.IsLoggedIn = App.ContextService.IsLoggedIn();
                _model.Requires2FA = App.ContextService.Requires2FA();
                _model.HasLoginToken = await App.ContextService.HasLoginTokenAsync();
                _model.HasPasswordItems = App.ContextService.HasPasswordItems();
                _model.PasswordItems = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }
}
