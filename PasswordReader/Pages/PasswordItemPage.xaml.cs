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