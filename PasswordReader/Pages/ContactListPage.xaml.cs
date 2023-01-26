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
using System.Collections.ObjectModel;

namespace PasswordReader.Pages;

public partial class ContactListPage : ContentPage
{
    private ContextViewModel _model;

    public ContactListPage()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        App.ContextService.StartPage = "//contactlist";
        if (_model.ContactItems == null || _model.HasErrorMessage)
        {
            _model.ContactItems = new ObservableCollection<ContactItemViewModel>();
            await GetContactItemsAsync();
        }
    }

    private async Task GetContactItemsAsync()
    {
        if (!_model.IsLoggedIn)
        {
            _model.ErrorMessage = "Du bist nicht mehr angemeldet.";
            return;
        }
        try
        {
            _model.IsRunning = true;
            var contactItems = await App.ContextService.GetContactItemsAsync();
            foreach (var contactItem in contactItems.OrderBy(x => x.name))
            {
                _model.ContactItems.Add(new ContactItemViewModel
                {
                    Id = contactItem.id,
                    Name = contactItem.name,
                    Address = contactItem.address,
                    Phone = contactItem.phone,
                    Email = contactItem.email,
                    Note = contactItem.note,
                    Birthday = contactItem.birthday
                });
            }
            _model.IsRunning = false;
            _model.ErrorMessage = "";
        }
        catch (Exception ex)
        {
            _model.IsRunning = false;
            _model.ErrorMessage = ex.Message;
        }
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            if (_model.SelectedContactItem == null)
            {
                return;
            }
            var navigationParameter = new Dictionary<string, object>()
            {
                { "item", _model.SelectedContactItem }
            };
            await Shell.Current.GoToAsync("contactitem", navigationParameter);
            _model.SelectedContactItem = null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

    private async void NewContact_Clicked(object sender, EventArgs e)
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
        _model.IsRunning = true;
        long nextId = 1;
        if (_model.ContactItems.Any())
        {
            nextId = _model.ContactItems.Max(x => x.Id) + 1;
        }
        var contactitemmodel = new ContactItemViewModel
        {
            Id = nextId,
            Name = string.Empty,
            Address = string.Empty,
            Email = string.Empty,
            Phone = string.Empty,
            Note = string.Empty,
            Birthday = string.Empty
        };
        int pos = 0;
        foreach (var previtem in _model.ContactItems)
        {
            if (previtem.Name.CompareTo(contactitemmodel.Name) >= 0)
            {
                break;
            }
            pos++;
        }
        _model.ContactItems.Insert(pos, contactitemmodel);
        try
        {
            await _model.UploadContactItemsAsync();
            _model.IsRunning = false;
            _model.SelectedContactItem = contactitemmodel;
        }
        catch (Exception ex)
        {
            if (_model.IsLoggedIn)
            {
                // remove added item if not saved
                _model.ContactItems.Remove(contactitemmodel);
            }
            _model.IsRunning = false;
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }

}