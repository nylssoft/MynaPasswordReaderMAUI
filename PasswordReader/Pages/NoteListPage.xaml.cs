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
    private ContextViewModel _model;

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
        _model.SelectedNoteItem = new();
        if (App.ContextService.IsLoggedIn())
        {
            await Decode();
        }
    }

    private async Task Decode()
    {
        try
        {
            var noteItems = await App.ContextService.GetNotesAsync();
            _model.NoteItems = new ObservableCollection<NoteItemViewModel>();
            foreach (var noteItem in noteItems)
            {
                _model.NoteItems.Add(new NoteItemViewModel
                {
                    Id = noteItem.id,
                    Title = noteItem.title
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void NoteItem_Clicked(object sender, EventArgs e)
    {
        Button b = sender as Button;
        if (b?.BindingContext is NoteItemViewModel n)
        {
            try
            {
                var note = await App.ContextService.GetNoteAsync(n.Id);
                _model.SelectedNoteItem = new();
                _model.SelectedNoteItem.Id = note.id;
                _model.SelectedNoteItem.Title = note.title;
                _model.SelectedNoteItem.Content = note.content;
                _model.SelectedNoteItem.LastModified = note.lastModifiedUtc.Value.ToLocalTime().ToString("f", new CultureInfo("de-DE"));
                var navigationParameter = new Dictionary<string, object>()
                {
                    { "item", _model.SelectedNoteItem }
                };
                await Shell.Current.GoToAsync("noteitem", navigationParameter);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    private async void NewNoteItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            var id = await App.ContextService.CreateNoteAsync();
            var note = await App.ContextService.GetNoteAsync(id);
            _model.NoteItems.Insert(0, new NoteItemViewModel
            {
                Id = note.id,
                Title = note.title
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}