using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

[QueryProperty("Item", "item")]
public partial class PasswordItemPage : ContentPage
{
    private PasswordItemViewModel _model;
    
    private const string HIDDEN = "*********";

    public PasswordItemPage()
	{
		InitializeComponent();
        _model = new PasswordItemViewModel();
        BindingContext = _model;
    }

    private PasswordItemViewModel _item;
    public PasswordItemViewModel Item
    {
        get => _item;
        set
        {
            if (_item == value)
            {
                return;
            }
            _item = value;
            UpdateModel();
        }
    }

    private void UpdateModel()
    {
        if (_item == null)
        {
            return;
        }
        _model.Name = _item.Name;
        _model.ImageUrl = _item.ImageUrl;
        _model.Url = _item.Url;
        _model.Login = _item.Login;
        _model.Description = _item.Description;
        _model.Password = HIDDEN;
        _model.StatusMessage = "";
    }

    private async Task CopyToClipboard(string txt, string property)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(txt);
            _model.StatusMessage = $"{property} in die Zwischenablage kopiert.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void CopyUrl_Clicked(object sender, EventArgs e)
    {
        await CopyToClipboard(_model.Url, "URL");
    }

    private async void CopyLogin_Clicked(object sender, EventArgs e)
    {
        await CopyToClipboard(_model.Login, "Benutzername");
    }

    private async void CopyPassword_Clicked(object sender, EventArgs e)
    {
        try
        {
            var pwd = await App.ContextService.DecodePasswordAsync(_item.Password);
            await CopyToClipboard(pwd, "Kennwort");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void CopyDescription_Clicked(object sender, EventArgs e)
    {
        await CopyToClipboard(_model.Description, "Beschreibung");
    }

    private async void ShowPassword_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_model.Password == HIDDEN)
            {
                var pwd = await App.ContextService.DecodePasswordAsync(_item.Password);
                _model.Password = pwd;
                _model.StatusMessage = "Kennwort angezeigt.";
            }
            else
            {
                _model.Password = HIDDEN;
                _model.StatusMessage = "Kennwort verborgen.";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}