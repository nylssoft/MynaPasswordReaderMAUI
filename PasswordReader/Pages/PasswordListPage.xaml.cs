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
        if (_model.PasswordItems == null)
        {
            _model.PasswordItems = new ObservableCollection<PasswordItemViewModel>();
            if (App.ContextService.HasPasswordItems())
            {
                await Decode();
            }
        }
    }

    private async Task Decode()
    {
        try
        {
            _model.IsRunning = true;
            var passwordItems = await App.ContextService.GetPasswordItemsAsync();
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
            _model.IsRunning = false;
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
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

    private async void NewPassword_Clicked(object sender, EventArgs e)
    {
        _model.IsRunning = true;
        var pwditemmodel = new PasswordItemViewModel
        {
            Name = "Neu",
            Description = "",
            Login = "",
            Password = "",
            Url = ""
        };
        int pos = 0;
        foreach (var previtem in _model.PasswordItems)
        {
            if (previtem.Name.CompareTo(pwditemmodel.Name) >= 0)
            {
                break;
            }
            pos++;
        }
        _model.PasswordItems.Insert(pos, pwditemmodel);
        try
        {
            await _model.UploadPasswordItemsAsync();
            _model.IsRunning = false;
            _model.SelectedPasswordItem = pwditemmodel;
        }
        catch (Exception ex)
        {
            // remove added item if not saved
            _model.PasswordItems.Remove(pwditemmodel);
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}