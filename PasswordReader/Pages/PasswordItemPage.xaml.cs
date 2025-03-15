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
using PasswordReader.Services;
using PasswordReader.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace PasswordReader.Pages;

[QueryProperty("Item", "item")]
public partial class PasswordItemPage : ContentPage
{
    private readonly PasswordItemViewModel _model;

    private string _encryptedPassword;

    private readonly IDispatcherTimer _timer;

    private const string HIDDEN = "*********";

    public PasswordItemPage()
    {
        InitializeComponent();
        _model = new PasswordItemViewModel();
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(2);
        _timer.Tick += ClearStatusMessage;
        BindingContext = _model;
    }

    private void ClearStatusMessage(object sender, EventArgs e)
    {
        _timer.Stop();
        _model.StatusMessage = "";
    }

    private void SetStatusMessage(string txt)
    {
        _model.StatusMessage = txt;
        _timer.Start();
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
        _model.IsUpdating = true;
        _model.Name = _item.Name;
        _model.ImageUrl = _item.ImageUrl;
        _model.Url = _item.Url;
        _model.Login = _item.Login;
        _model.Description = _item.Description;
        _model.Password = string.IsNullOrEmpty(_item.Password) ? "" : HIDDEN;
        _model.StatusMessage = "";
        _encryptedPassword = _item.Password;
        passwordEntry.IsPassword = true;
        _model.Changed = false;
        _model.IsUpdating = false;
    }

    private async Task CopyToClipboard(string txt, string property)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(txt);
            SetStatusMessage($"{property} in die Zwischenablage kopiert.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (_model.Changed && App.ContextViewModel.IsLoggedIn)
        {
            if (!await DisplayAlert("Zurück", "Das Kennwort wurde nicht gespeichert. Willst Du die Seite wirklich verlassen?", "Ja", "Nein"))
            {
                return;
            }
            _model.Changed = false;
            App.ContextService.PasswordChanged = false;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OpenUrl_Clicked(object sender, EventArgs e)
    {
        string url = _model.Url;
        if (!string.IsNullOrEmpty(url))
        {
            string pattern = @"^https?://";
            var m = System.Text.RegularExpressions.Regex.Match(_model.Url, pattern);
            if (!m.Success)
            {
                url = $"https://{url}";
            }
            try
            {
                var opened = await Browser.OpenAsync(url, BrowserLaunchMode.External);
                if (!opened)
                {
                    await DisplayAlert("Fehler", $"Die URL '{url}' kann nicht geöffnet werden.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
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
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
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
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        _model.IsUpdating = true;
        try
        {
            if (passwordEntry.IsPassword)
            {
                if (_model.Password == HIDDEN)
                {
                    var pwd = await App.ContextService.DecodePasswordAsync(_encryptedPassword);
                    _model.Password = pwd;
                }
                SetStatusMessage("Kennwort angezeigt.");
                showPasswordButton.Source = "eyeslashdark.png";
                passwordEntry.IsPassword = false;
            }
            else
            {
                if (_model.Password != HIDDEN && !string.IsNullOrEmpty(_model.Password))
                {
                    _encryptedPassword = await App.ContextService.EncodePasswordAsync(_model.Password);
                    _model.Password = HIDDEN;
                }
                SetStatusMessage("Kennwort verborgen.");
                showPasswordButton.Source = "eyedark.png";
                passwordEntry.IsPassword = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
        _model.IsUpdating = false;
    }

    private async void SavePassword_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        _model.IsRunning = true;
        var backup = new PasswordItemViewModel
        {
            Name = _item.Name,
            Login = _item.Login,
            Password = _item.Password,
            Url = _item.Url,
            Description = _item.Description,
            ImageUrl = _item.ImageUrl
        };
        var nameChanged = _item.Name != _model.Name;
        var urlChanged = _item.Url != _model.Url;
        _item.Name = _model.Name;
        _item.Login = _model.Login;
        if (string.IsNullOrEmpty(_model.Password))
        {
            _item.Password = "";
        }
        else
        {
            _item.Password = _model.Password == HIDDEN ? _encryptedPassword : await App.ContextService.EncodePasswordAsync(_model.Password);
        }
        _item.Url = _model.Url;
        _item.Description = _model.Description;
        try
        {
            await App.ContextViewModel.UploadPasswordItemsAsync();
            if (urlChanged)
            {
                // fetch image url again
                PasswordItem p = new()
                {
                    Url = _item.Url
                };
                _item.ImageUrl = p.ImageUrl;
            }
            if (nameChanged)
            {
                // insert into new position, items are ordered by name
                App.ContextViewModel.PasswordItems.Remove(_item);
                int pos = 0;
                foreach (var previtem in App.ContextViewModel.PasswordItems)
                {
                    if (previtem.Name.CompareTo(_item.Name) >= 0)
                    {
                        break;
                    }
                    pos++;
                }
                App.ContextViewModel.PasswordItems.Insert(pos, _item);
            }
            App.ContextService.PasswordChanged = false;
            _model.Changed = false;
            _model.IsRunning = false;
            SetStatusMessage("Kennwort gespeichert.");
        }
        catch (Exception ex)
        {
            // restore previous item if not saved
            _item.Name = backup.Name;
            _item.Login = backup.Login;
            _item.Password = backup.Password;
            _item.Url = backup.Url;
            _item.Description = backup.Description;
            _item.ImageUrl = backup.ImageUrl;
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void DeletePassword_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        if (await DisplayAlert("Kennwort löschen", "Willst Du das Kennwort wirklich löschen?", "Ja", "Nein"))
        {
            _model.IsRunning = true;
            int pos = 0;
            foreach (var pwditem in App.ContextViewModel.PasswordItems)
            {
                if (pwditem == _item)
                {
                    break;
                }
                pos++;
            }
            App.ContextViewModel.PasswordItems.Remove(_item);
            try
            {
                await App.ContextViewModel.UploadPasswordItemsAsync();
                _model.IsRunning = false;
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                if (App.ContextViewModel.IsLoggedIn)
                {
                    // insert item again if not saved
                    App.ContextViewModel.PasswordItems.Insert(pos, _item);
                }
                _model.IsRunning = false;
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    private void Password_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_model.IsUpdating && App.ContextViewModel.IsLoggedIn)
        {
            _model.Changed = true;
            App.ContextService.PasswordChanged = true;
        }
    }
}