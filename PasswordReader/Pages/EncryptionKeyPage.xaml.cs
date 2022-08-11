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

public partial class EncryptionKeyPage : ContentPage
{
    private ContextViewModel _model;

    private const string HIDDEN = "*********";

    public EncryptionKeyPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (await App.ContextService.HasEncryptionKeyAsync())
        {
            _model.EncryptionKey = HIDDEN;
        }
    }

    private async void ShowEncryptionKey_Clicked(object sender, EventArgs e)
    {
        if (_model.EncryptionKey == HIDDEN)
        {
            _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            showEncryptionKeyButton.Source = Application.Current.RequestedTheme == AppTheme.Light ? "eyeslash.png" : "eyeslashdark.png";
        }
        else if (await App.ContextService.HasEncryptionKeyAsync())
        {
            _model.EncryptionKey = HIDDEN;
            showEncryptionKeyButton.Source = Application.Current.RequestedTheme == AppTheme.Light ? "eye.png" : "eyedark.png";
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        await Save();
    }

    private async Task Save()
    {
        try
        {
            if (_model.EncryptionKey == HIDDEN)
            {
                _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            }
            await App.ContextService.SetEncryptionKeyAsync(_model.EncryptionKey);
            _model.PasswordItems = null; // force reload
            await Shell.Current.GoToAsync("//passwordlist");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

}