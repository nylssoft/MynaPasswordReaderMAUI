using Java.Util;
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
        App.ContextService.StartPage = "//photoframe";
        Uri uri = new($"https://www.stockfleth.eu/webpack/tsphotoframe/index.html?v={UUID.RandomUUID()}", UriKind.RelativeOrAbsolute);
        if (_model.IsLoggedIn)
        {
            string familyAccessToken = await App.ContextService.GetFamilyAccessTokenAsync();
            if (familyAccessToken != null)
            {
                CookieContainer cookieContainer = new();
                Cookie cookie = new()
                {
                    Name = "familyaccesstoken",
                    Expires = DateTime.Now.AddDays(1),
                    Value = familyAccessToken,
                    Domain = uri.Host,
                    Secure = true,
                    Path = "/"
                };
                cookieContainer.Add(uri, cookie);
                webView.Cookies = cookieContainer;
            }
        }
        webView.Source = new UrlWebViewSource { Url = uri.ToString() };
        DeviceDisplay.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        DeviceDisplay.KeepScreenOn = false;
    }

}