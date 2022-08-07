using PasswordReader.ViewModels;
using System.Collections.ObjectModel;

namespace PasswordReader.Pages;

public partial class PasswordListPage : ContentPage
{
    private ContextViewModel _model;

    public PasswordListPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
    }

    private async void Decode_Clicked(object sender, EventArgs e)
	{
        try
        {
            _model.PasswordItems = new ObservableCollection<PasswordItemViewModel>();
            var passwordItems = await App.ContextService.DecodePasswordItemsAsync();
            foreach (var pwdItem in passwordItems)
            {
                _model.PasswordItems.Add(new PasswordItemViewModel {
                    Name = pwdItem.Name,
                    ImageUrl = pwdItem.ImageUrl,
                    Url = pwdItem.Url,
                    Login = pwdItem.Login,
                    Description = pwdItem.Description,
                    Password = pwdItem.Password
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            if (_model.SelectedPasswordItem == null)
            {
                return;
            }
            var navigationParameter = new Dictionary<string, object>()
            {
                { "item", _model.SelectedPasswordItem }
            };
            await Shell.Current.GoToAsync("passworditem", navigationParameter);
            _model.SelectedPasswordItem = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private void ListView_ItemSelected_1(object sender, SelectedItemChangedEventArgs e)
    {

    }

    private void ListView_ItemSelected_2(object sender, SelectedItemChangedEventArgs e)
    {

    }
}