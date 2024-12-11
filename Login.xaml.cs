namespace MauiRfidSample;

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Http;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
        BindingContext = this;
        Title = "Login";
    }

    //private async void OnLoginButtonClicked(object sender, EventArgs e)
    //{
    //    string username = UsernameEntry.Text;
    //    string password = PasswordEntry.Text;

    //    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    //    {
    //        await DisplayAlert("Error", "Please enter both username and password.", "OK");
    //        return;
    //    }

    //    //keep this after the null check or else it crashes
    //    bool isEmail = Regex.IsMatch(UsernameEntry.Text,
    //        @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
    //        RegexOptions.IgnoreCase);

    //    if (!isEmail)
    //    {
    //        await DisplayAlert("Error", "Please use a valid email address.", "OK");
    //        return;
    //    }
    //    await DisplayAlert("Success", $"Login Successful", "OK");
    //    Navigation.PushAsync(new MainPage());
    //}
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string clientId = "3MVG9FINO1nsxRuCKdhiAIOm6bjbYgBzJOWu9V7zNWfXv.W7NNd6a5zOXrIN3gVQxpS48QA0Qo6zbweC4T8lH"; 
        string clientSecret = "A72411D38F432952D201A224FB7C57C2BA7BE516278D87EB4FD7DC05F5C1AF65";
        string redirectUri = "myapp://oauth/callback";
        string authUrl = "https://rfidmaui-dev-ed.develop.my.salesforce.com/services/oauth2/authorize";
        string tokenUrl = "https://rfidmaui-dev-ed.develop.my.salesforce.com/services/oauth2/token";


        string loginUrl = $"{authUrl}?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code";

        await Browser.Default.OpenAsync(loginUrl, BrowserLaunchMode.SystemPreferred);
    }
}