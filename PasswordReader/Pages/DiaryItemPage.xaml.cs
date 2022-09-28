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

[QueryProperty("Item", "item")]
public partial class DiaryItemPage : ContentPage
{
    private readonly DiaryItemViewModel _model;

    private readonly IDispatcherTimer _timer;

    public DiaryItemPage()
    {
        InitializeComponent();
        _model = new DiaryItemViewModel();
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

    private DiaryItemViewModel _item;
    public DiaryItemViewModel Item
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
        _model.Date = _item.Date;
        _model.Entry = _item.Entry;
        _model.Changed = false;
        _model.IsUpdating = false;
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        if (_model.Changed && App.ContextViewModel.IsLoggedIn)
        {
            if (!await DisplayAlert("Zurück", "Der Tagebucheintrag wurde nicht gespeichert. Willst Du die Seite wirklich verlassen?", "Ja", "Nein"))
            {
                return;
            }
            _model.Changed = false;
            App.ContextService.DiaryChanged = false;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        try
        {
            _model.IsRunning = true;
            await App.ContextService.SaveDiaryAsync(_model.Date.Year, _model.Date.Month, _model.Date.Day, _model.Entry);
            _model.Changed = false;
            App.ContextService.DiaryChanged = false;
            _model.IsRunning = false;
            SetStatusMessage("Tagebucheintrag gespeichert.");
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        if (!App.ContextViewModel.IsLoggedIn)
        {
            await DisplayAlert("Fehler", "Du bist nicht mehr angemeldet.", "OK");
            return;
        }
        try
        {
            if (await DisplayAlert("Tagebucheintrag löschen", "Willst Du den Tagebucheintrag wirklich löschen?", "Ja", "Nein"))
            {
                _model.IsRunning = true;
                await App.ContextService.SaveDiaryAsync(_model.Date.Year, _model.Date.Month, _model.Date.Day, "");
                _model.Changed = false;
                App.ContextService.DiaryChanged = false;
                _model.IsRunning = false;
                SetStatusMessage("Tagebucheintrag gelöscht.");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private void Diary_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_model.IsUpdating && App.ContextViewModel.IsLoggedIn)
        {
            _model.Changed = true;
            App.ContextService.DiaryChanged = true;
        }
    }
}