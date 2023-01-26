/*
    Myna Password Reader MAUI
    Copyright (C) 2023 Niels Stockfleth

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
public partial class ContactItemPage : ContentPage
{
    private readonly ContactItemViewModel _model;

    private readonly IDispatcherTimer _timer;

    public ContactItemPage()
	{
		InitializeComponent();
        _model = new ContactItemViewModel();
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

    private ContactItemViewModel _item;
    public ContactItemViewModel Item
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
        _model.Id = _item.Id;
        _model.Name = _item.Name;
        _model.Address = _item.Address;
        _model.Birthday = _item.Birthday;
        _model.Email = _item.Email;
        _model.Phone = _item.Phone;
        _model.Note = _item.Note;
        _model.StatusMessage = "";
        _model.Changed = false;
        _model.IsUpdating = false;
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (_model.Changed && App.ContextViewModel.IsLoggedIn)
        {
            if (!await DisplayAlert("Zurück", "Der Kontakt wurden nicht gespeichert. Willst Du die Seite wirklich verlassen?", "Ja", "Nein"))
            {
                return;
            }
            _model.Changed = false;
            App.ContextService.ContactChanged = false;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void SaveContact_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        _model.IsRunning = true;
        var backup = new ContactItemViewModel
        {
            Id = _item.Id,
            Name = _item.Name,
            Address = _item.Address,
            Phone = _item.Phone,
            Birthday = _item.Birthday,
            Email = _item.Email,
            Note = _item.Note
        };
        var nameChanged = _item.Name != _model.Name;
        _item.Id = _model.Id;
        _item.Name = _model.Name;
        _item.Address = _model.Address;
        _item.Phone = _model.Phone;
        _item.Birthday= _model.Birthday;
        _item.Email = _model.Email;
        _item.Note = _model.Note;
        try
        {
            await App.ContextViewModel.UploadContactItemsAsync();
            if (nameChanged)
            {
                // insert into new position, items are ordered by name
                App.ContextViewModel.ContactItems.Remove(_item);
                int pos = 0;
                foreach (var previtem in App.ContextViewModel.ContactItems)
                {
                    if (previtem.Name.CompareTo(_item.Name) >= 0)
                    {
                        break;
                    }
                    pos++;
                }
                App.ContextViewModel.ContactItems.Insert(pos, _item);
            }
             App.ContextService.ContactChanged = false;
            _model.Changed = false;
            _model.IsRunning = false;
            SetStatusMessage("Kontakt gespeichert.");
        }
        catch (Exception ex)
        {
            // restore previous item if not saved
            _item.Id = backup.Id;
            _item.Name = backup.Name;
            _item.Address = backup.Address;
            _item.Phone = backup.Phone;
            _item.Birthday = backup.Birthday;
            _item.Email = backup.Email;
            _item.Note = backup.Note;
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void DeleteContact_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        if (await DisplayAlert("Kontakt löschen", "Willst Du den Kontakt wirklich löschen?", "Ja", "Nein"))
        {
            _model.IsRunning = true;
            int pos = 0;
            foreach (var contactitem in App.ContextViewModel.ContactItems)
            {
                if (contactitem == _item)
                {
                    break;
                }
                pos++;
            }
            App.ContextViewModel.ContactItems.Remove(_item);
            try
            {
                await App.ContextViewModel.UploadContactItemsAsync();
                _model.IsRunning = false;
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                if (App.ContextViewModel.IsLoggedIn)
                {
                    // insert item again if not saved
                    App.ContextViewModel.ContactItems.Insert(pos, _item);
                }
                _model.IsRunning = false;
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    private void Contact_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_model.IsUpdating && App.ContextViewModel.IsLoggedIn)
        {
            _model.Changed = true;
            App.ContextService.ContactChanged = true;
        }
    }
}