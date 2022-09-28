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

public partial class DiaryPage : ContentPage
{
	private readonly DiaryViewModel _model;

	public DiaryPage()
	{
		InitializeComponent();
		_model = new DiaryViewModel();
        BindingContext = _model;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await UpdateCalendarAsync();
    }

    private async void GraphicsView_StartInteraction(object sender, TouchEventArgs e)
	{
        if (e.Touches.Length == 1)
        {
            var touch = e.Touches[0];
            var gd = graphicsView.Drawable as DiaryPageDrawable;
            var day = gd.GetDay(touch.X, touch.Y);
            if (day != null)
            {
                await GotoEntryAsync(day.Value);
            }
        }
    }

    private async Task GotoEntryAsync(int day)
    {
        _model.ErrorMessage = "";
        if (!App.ContextViewModel.IsLoggedIn)
        {
            _model.ErrorMessage = "Du bist nicht angemeldet.";
            return;
        }
        try
        {
            _model.IsRunning = true;
            var item = new DiaryItemViewModel();
            if (_model.Days == null || !_model.Days.Contains(day))
            {
                if (string.IsNullOrEmpty(await App.ContextService.GetEncryptionKeyAsync())) throw new ArgumentException("Es wurde kein Schlüssel konfiguriert.");
                item.Date = new DateTime(_model.Year, _model.Month, day);
                item.Entry = "";
            }
            else
            {
                var diary = await App.ContextService.GetDiaryAsync(_model.Year, _model.Month, day);
                item.Date = diary.date.Value;
                item.Entry = diary.entry;
            }
            var navigationParameter = new Dictionary<string, object>()
            {
                { "item", item }
            };
            await Shell.Current.GoToAsync("diaryitem", navigationParameter);
            _model.IsRunning = false;
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            _model.ErrorMessage = ex.Message;
        }
    }

    private async void Left_Clicked(object sender, EventArgs e)
	{
		var next = _model.Month - 1;
		if (next <= 0)
		{
			next = 12;
			_model.Year -= 1;
		}
        _model.Month = next;
        await UpdateCalendarAsync();
    }

    private async void Right_Clicked(object sender, EventArgs e)
    {
        var next = _model.Month + 1;
        if (next > 12)
        {
            next = 1;
            _model.Year += 1;
        }
        _model.Month = next;
        await UpdateCalendarAsync();
    }

	private async Task UpdateCalendarAsync()
	{
        _model.ErrorMessage = "";
        _model.Days = null;
        try
        {
            _model.Days = await App.ContextService.GetDiaryDaysAsync(_model.Year, _model.Month);
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = ex.Message;
        }
        var gd = graphicsView.Drawable as DiaryPageDrawable;
        gd.Days = _model.Days;
        gd.Month = _model.Month;
        gd.Year = _model.Year;
        graphicsView.Invalidate();
    }
}