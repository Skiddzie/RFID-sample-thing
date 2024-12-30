namespace MauiRfidSample;

using System.Text.Json;
using System.Net.Http;
using Microsoft.Maui.Authentication;
using System.Diagnostics;

public partial class Login : ContentPage
{
    public const string ClientId = "3MVG9FINO1nsxRuCKdhiAIOm6bjbYgBzJOWu9V7zNWfXv.W7NNd6a5zOXrIN3gVQxpS48QA0Qo6zbweC4T8lH";
    private const string ClientSecret = "A72411D38F432952D201A224FB7C57C2BA7BE516278D87EB4FD7DC05F5C1AF65";
    private const string RedirectUri = "myapp://oauth/callback";
    private const string AuthUrl = "https://rfidmaui-dev-ed.develop.my.salesforce.com/services/oauth2/authorize";
    private const string TokenUrl = "https://rfidmaui-dev-ed.develop.my.salesforce.com/services/oauth2/token";
    private AscentWebService _webService;
    public Login()
    {
        InitializeComponent();
        _webService = App.SharedAscentWebService;
        BindingContext = this;
        Title = "Login";
    }
    private async void OnNavigateButtonClicked(object sender, EventArgs e)
    {
        //bool success = SecureStorage.Default.Remove("AccessToken"); // 
        _webService.SetOAuthToken(null, null);
        await Navigation.PushAsync(new MainPage());
    }
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        try
        {
            string loginUrl = $"{AuthUrl}?client_id={Uri.EscapeDataString(ClientId)}" +
                              $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                              "&response_type=code&prompt=login";

            Trace.WriteLine($"Opening browser for login: {loginUrl}");

            await Browser.Default.OpenAsync(new Uri(loginUrl), BrowserLaunchMode.SystemPreferred);

            Trace.WriteLine("Login page opened in the browser.");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error opening browser: {ex.Message}");
            await DisplayAlert("Error", "Unable to open the browser. Please try again.", "OK");
        }
    }
}