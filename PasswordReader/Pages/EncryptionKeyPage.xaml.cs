/*
    Myna Password Reader MAUI
    Copyright (C) 2022-2023 Niels Stockfleth

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
using APIServer.PasswordGenerator;
using PasswordReader.ViewModels;

namespace PasswordReader.Pages;

public partial class EncryptionKeyPage : ContentPage
{
    private ContextViewModel _model;

    private string _savedEncryptionKey;

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
        _savedEncryptionKey = "";
        if (await App.ContextService.HasEncryptionKeyAsync())
        {
            _savedEncryptionKey = await App.ContextService.GetEncryptionKeyAsync();
            _model.EncryptionKey = HIDDEN;
        }
        encryptionKeyEntry.IsPassword = true;
        encryptionKeyEntry.IsReadOnly = true;
        showEncryptionKeyButton.Source = "eyedark.png";
    }

    private void ShowEncryptionKey_Clicked(object sender, EventArgs e)
    {
        if (encryptionKeyEntry.IsPassword)
        {
            if (_model.EncryptionKey == HIDDEN)
            {
                _model.EncryptionKey = _savedEncryptionKey;
            }
            encryptionKeyEntry.IsPassword = false;
            encryptionKeyEntry.IsReadOnly = false;
            showEncryptionKeyButton.Source = "eyeslashdark.png";
        }
        else
        {
            if (_model.EncryptionKey != HIDDEN && !string.IsNullOrEmpty(_model.EncryptionKey))
            {
                _savedEncryptionKey = _model.EncryptionKey;
                _model.EncryptionKey = HIDDEN;
            }
            encryptionKeyEntry.IsPassword = true;
            encryptionKeyEntry.IsReadOnly = true;
            showEncryptionKeyButton.Source = "eyedark.png";
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
            var encryptionKey = _model.EncryptionKey;
            if (encryptionKey == HIDDEN)
            {
                encryptionKey = _savedEncryptionKey;
            }
            await App.ContextService.SetEncryptionKeyAsync(encryptionKey);
            // force reload
            _model.SelectedPasswordItem = null;
            _model.SelectedNoteItem = null;
            _model.SelectedContactItem = null;
            _model.PasswordItems = null;
            _model.NoteItems = null;
            _model.ContactItems = null;
            await Shell.Current.GoToAsync(App.ContextService.StartPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Generate_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_model.EncryptionKey))
            {
                var pwdgen = new PwdGen { Length = 24 };
                _model.EncryptionKey = pwdgen.Generate();
                _savedEncryptionKey = _model.EncryptionKey;
                if (encryptionKeyEntry.IsPassword)
                {
                    encryptionKeyEntry.IsPassword = false;
                    encryptionKeyEntry.IsReadOnly = false;
                    showEncryptionKeyButton.Source = "eyeslashdark.png";
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

}