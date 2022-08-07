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

public partial class LoginPage : ContentPage
{
    private ContextViewModel _model;

	public LoginPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    private async void LoginToken_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.ContextService.LoginWithTokenAsync();
            await UpdateModel();
            await GotoNextPage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Login_Clicked(object sender, EventArgs e)
	{
		try
		{
            await App.ContextService.LoginAsync(_model.Username, _model.Password);
            await UpdateModel();
            await GotoNextPage();
        }
        catch (Exception ex)
		{
			await DisplayAlert("Fehler", ex.Message, "OK");
		}
    }

    private async Task UpdateModel()
    {
        _model.Username = App.ContextService.GetUsername();
        _model.UserPhotoUrl = App.ContextService.GetUserPhotoUrl();
        _model.Password = "";
        _model.SecurityCode = "";
        _model.EncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
        _model.IsLoggedIn = App.ContextService.IsLoggedIn();
        _model.Requires2FA = App.ContextService.Requires2FA();
        _model.HasLoginToken = await App.ContextService.HasLoginTokenAsync();
        _model.HasPasswordItems = App.ContextService.HasPasswordItems();
    }

    private async Task GotoNextPage()
    {
        string page;
        if (App.ContextService.Requires2FA())
        {
            page = "//login2fa";
        }
        else if (string.IsNullOrEmpty(_model.EncryptionKey))
        {
            page = "//encryptionkey";
        }
        else
        {
            page = "//passwordlist";
        }
        await Shell.Current.GoToAsync(page);
    }

    private void Username_Completed(object sender, EventArgs e)
    {
        entryPassword.Focus();
    }

    private void Password_Completed(object sender, EventArgs e)
    {
        // does not work, keyboard remains open
        // https://github.com/dotnet/maui/pull/7635
        // Login_Clicked(sender, e);
    }

}