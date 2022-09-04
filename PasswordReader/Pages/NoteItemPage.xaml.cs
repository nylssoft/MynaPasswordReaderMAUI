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
using System.Globalization;

namespace PasswordReader.Pages;

[QueryProperty("Item", "item")]
public partial class NoteItemPage : ContentPage
{
    private readonly NoteItemViewModel _model;

    private readonly IDispatcherTimer _timer;

    public NoteItemPage()
	{
		InitializeComponent();
        _model = new NoteItemViewModel();
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

    private NoteItemViewModel _item;
    public NoteItemViewModel Item
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
        _model.Title = _item.Title;
        _model.LastModified = _item.LastModified;
        _model.Content = _item.Content;
        _model.Changed = false;
        _model.IsUpdating = false;
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (_model.Changed && App.ContextViewModel.IsLoggedIn)
        {
            if (!await DisplayAlert("Zurück", "Die Notiz wurde nicht gespeichert. Willst Du die Seite wirklich verlassen?", "Ja", "Nein"))
            {
                return;
            }
            _model.Changed = false;
            App.ContextService.NoteChanged = false;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void DeleteNote_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        try
        {
            if (await DisplayAlert("Notiz löschen", "Willst Du die Notiz wirklich löschen?", "Ja", "Nein"))
            {
                _model.IsRunning = true;
                await App.ContextService.DeleteNoteAsync(_model.Id);
                NoteItemViewModel delitem = null;
                foreach (var noteitem in App.ContextViewModel.NoteItems)
                {
                    if (noteitem.Id == _model.Id)
                    {
                        delitem = noteitem;
                        break;
                    }
                }
                if (delitem != null)
                {
                    App.ContextViewModel.NoteItems.Remove(delitem);
                }
                await Shell.Current.GoToAsync("..");
                _model.IsRunning = false;
            }
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void SaveNote_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        try
        {
            _model.IsRunning = true;
            var lastModifiedUtc = await App.ContextService.UpdateNoteAsync(_model.Id, _model.Title, _model.Content);
            _model.LastModified = lastModifiedUtc.ToLocalTime().ToString("f", new CultureInfo("de-DE"));
            _model.Changed = false;
            App.ContextService.NoteChanged = false;
            foreach (var noteitem in App.ContextViewModel.NoteItems)
            {
                if (noteitem.Id == _model.Id)
                {
                    noteitem.Title = _model.Title;
                }
            }
            _model.IsRunning = false;
            SetStatusMessage("Notiz gespeichert.");
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private void Note_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_model.IsUpdating && App.ContextViewModel.IsLoggedIn)
        {
            _model.Changed = true;
            App.ContextService.NoteChanged = true;
        }
    }
}