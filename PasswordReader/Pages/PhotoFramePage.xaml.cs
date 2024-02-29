/*
    Myna Password Reader MAUI
    Copyright (C) 2024 Niels Stockfleth

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
using System.Net;

namespace PasswordReader.Pages;

public partial class PhotoFramePage : ContentPage
{

    private readonly ContextViewModel _model;

    public PhotoFramePage()
    {
        InitializeComponent();
        _model = App.ContextViewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        Uri uri = new($"https://www.stockfleth.eu/webpack/tsphotoframe/index.html?v={DateTime.Now.Ticks}", UriKind.RelativeOrAbsolute);
        string familyAccessToken = "";
        if (_model.IsLoggedIn)
        {
            familyAccessToken = await App.ContextService.GetFamilyAccessTokenAsync() ?? "";
        }
        CookieContainer cookieContainer = new();
        Cookie cookie = new()
        {
            Name = "familyaccesstoken",
            Value = familyAccessToken,
            Domain = uri.Host,
            Secure = true
        };
        cookieContainer.Add(uri, cookie);
        // crashes currently on windows
#if ANDROID
        webView.Cookies = cookieContainer;
#endif
        webView.Source = new UrlWebViewSource { Url = uri.ToString() };
        DeviceDisplay.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        DeviceDisplay.KeepScreenOn = false;
    }

}