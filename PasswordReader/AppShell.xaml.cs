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
using PasswordReader.Pages;
using PasswordReader.ViewModels;

namespace PasswordReader;

public partial class AppShell : Shell
{
    private ContextViewModel _model;
    private bool _init;

    public AppShell()
	{
		InitializeComponent();
        _model = App.ContextViewModel;
        BindingContext = _model;
        Routing.RegisterRoute("passworditem", typeof(PasswordItemPage));
        Routing.RegisterRoute("noteitem", typeof(NoteItemPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_init)
        {
            try
            {
                _init = true;
                await _model.InitAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }

    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);
        if (App.ContextService.NoteChanged)
        {
            ShellNavigatingDeferral token = args.GetDeferral();
            var leavePage = await DisplayAlert("Seite verlassen", "Die Änderungen wurden nicht gespeichert. Willst Du die Seite wirklich verlassen?", "Ja", "Nein");
            if (leavePage)
            {
                App.ContextService.NoteChanged = false;
            }
            else
            {
                args.Cancel();
            }
            token.Complete();
        }
    }
}
