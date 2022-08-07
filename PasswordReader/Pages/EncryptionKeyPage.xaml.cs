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

public partial class EncryptionKeyPage : ContentPage
{
    private ContextViewModel _model;

    public EncryptionKeyPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
	}

    private async void Save_Clicked(object sender, EventArgs e)
    {
        try
        {
            await App.ContextService.SetEncryptionKeyAsync(_model.EncryptionKey);
            await Shell.Current.GoToAsync("//passwordlist");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}