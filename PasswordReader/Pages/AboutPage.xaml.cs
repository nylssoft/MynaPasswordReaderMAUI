using Microsoft.Maui.ApplicationModel;
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
namespace PasswordReader.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
    }


    public string DisplayAppVersion
    {
        get
        {
            return $"Version {AppInfo.Current.VersionString} Build {AppInfo.Current.BuildString}";
        }
    }

    private async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
    {
        try
        {
            string url = "https://www.stockfleth.eu";
            var opened = await Browser.OpenAsync(url, BrowserLaunchMode.External);
            if (!opened)
            {
                await DisplayAlert("Fehler", $"Die URL '{url}' kann nicht ge�ffnet werden.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Fehler", ex.Message, "OK");
        }
    }
}