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
public partial class NoteItemPage : ContentPage
{
    private readonly NoteItemViewModel _model;

    public NoteItemPage()
	{
		InitializeComponent();
        _model = new NoteItemViewModel();
        BindingContext = _model;
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
        _model.Id = _item.Id;
        _model.Title = _item.Title;
        _model.LastModified = _item.LastModified;
        _model.Content = _item.Content;
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}