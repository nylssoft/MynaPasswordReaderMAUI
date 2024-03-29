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
using System.Globalization;

namespace PasswordReader.Pages;

public partial class NoteListPage : ContentPage
{
    private readonly ContextViewModel _model;

    public NoteListPage()
    {
        InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
        _model.SelectedNoteItem = new();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        App.ContextService.StartPage = "//notelist";
        _model.SelectedNoteItem = new();
        if (App.ContextService.IsLoggedIn())
        {
            await GetNotesAsync();
        }
    }

    private async Task GetNotesAsync()
    {
        if (!_model.IsLoggedIn)
        {
            _model.ErrorMessage = "Du bist nicht mehr angemeldet.";
            return;
        }
        try
        {
            if (_model.NoteItems == null || _model.HasErrorMessage)
            {
                _model.IsRunning = true;
                _model.NoteItems = [];
                var noteItems = await App.ContextService.GetNotesAsync();
                foreach (var noteItem in noteItems)
                {
                    _model.NoteItems.Add(new NoteItemViewModel
                    {
                        Id = noteItem.id,
                        Title = noteItem.title
                    });
                }
                _model.IsRunning = false;
                _model.ErrorMessage = "";
            }
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            _model.ErrorMessage = ex.Message;
        }
    }

    private async void NoteItem_Clicked(object sender, EventArgs e)
    {
        if (_model.HasErrorMessage)
        {
            return;
        }
        if (!_model.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        Button b = sender as Button;
        if (b?.BindingContext is NoteItemViewModel n)
        {
            try
            {
                _model.IsRunning = true;
                var note = await App.ContextService.GetNoteAsync(n.Id);
                _model.SelectedNoteItem = new()
                {
                    Id = note.id,
                    Title = note.title,
                    Content = note.content,
                    LastModified = note.lastModifiedUtc.Value.ToLocalTime().ToString("g", new CultureInfo("de-DE"))
                };
                var navigationParameter = new Dictionary<string, object>()
                {
                    { "item", _model.SelectedNoteItem }
                };
                await Shell.Current.GoToAsync("noteitem", navigationParameter);
                _model.IsRunning = false;
            }
            catch (Exception ex)
            {
                _model.IsRunning = false;
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    private async void NewNoteItem_Clicked(object sender, EventArgs e)
    {
        if (_model.HasErrorMessage)
        {
            return;
        }
        if (!_model.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        try
        {
            _model.IsRunning = true;
            var id = await App.ContextService.CreateNoteAsync();
            var note = await App.ContextService.GetNoteAsync(id);
            _model.NoteItems.Insert(0, new NoteItemViewModel
            {
                Id = note.id,
                Title = note.title
            });
            _model.IsRunning = false;
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}