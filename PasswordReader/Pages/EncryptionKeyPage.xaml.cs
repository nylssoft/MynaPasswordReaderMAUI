using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

public partial class EncryptionKeyPage : ContentPage
{
    private ContextViewModel _model;

    public EncryptionKeyPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    private async void Save_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.ContextService.SetEncryptionKeyAsync(_model.EncryptionKey);
            await Shell.Current.GoToAsync("//passwordlist");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}