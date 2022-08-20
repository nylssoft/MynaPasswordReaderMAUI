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
            _model.IsRunning = true;
            await App.ContextService.Login2FAAsync(_model.SecurityCode);
            await _model.InitAsync();
            await GotoNextPage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
        finally
        {
            _model.IsRunning = false;
        }
    }

    private async Task GotoNextPage()
    {
        string page;
        if (string.IsNullOrEmpty(_model.EncryptionKey))
        {
            page = "//encryptionkey";
        }
        else if (_model.HasPasswordItems)
        {
            page = "//passwordlist";
        }
        else
        {
            page = "//notelist";
        }
        await Shell.Current.GoToAsync(page);
    }
}