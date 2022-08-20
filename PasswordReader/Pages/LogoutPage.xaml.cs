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

public partial class LogoutPage : ContentPage
{
    private ContextViewModel _model;

    public LogoutPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        try
        {
            _model.IsRunning = true;
            await App.ContextService.LogoutAsync();
            _model.Username = App.ContextService.GetUsername();
            _model.UserPhotoUrl = App.ContextService.GetUserPhotoUrl();
            _model.Password = "";
            _model.SecurityCode = "";
            _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            _model.IsLoggedIn = App.ContextService.IsLoggedIn();
            _model.Requires2FA = App.ContextService.Requires2FA();
            _model.HasLoginToken = await App.ContextService.HasLoginTokenAsync();
            _model.HasPasswordItems = App.ContextService.HasPasswordItems();
            _model.PasswordItems = null;
            _model.SelectedPasswordItem = null;
            _model.NoteItems = null;
            _model.SelectedNoteItem = null;
            await Shell.Current.GoToAsync("//login");
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
}