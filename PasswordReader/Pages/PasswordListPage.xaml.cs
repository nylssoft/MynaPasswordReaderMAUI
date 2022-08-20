/*
    Myna Password Reader MAUI
    Copyright (C) 2022 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (_model.CanDecodePasswordItems && _model.PasswordItems == null)
        {
            await Decode();
        }
    }

    private async Task Decode()
    {
        try
        {
            var passwordItems = await App.ContextService.GetPasswordItemsAsync();
            _model.PasswordItems = new ObservableCollection<PasswordItemViewModel>();
            foreach (var pwdItem in passwordItems)
            {
                _model.PasswordItems.Add(new PasswordItemViewModel
                {
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
}