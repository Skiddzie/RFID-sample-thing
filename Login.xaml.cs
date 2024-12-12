namespace MauiRfidSample;

using System.Text.Json;
using System.Net.Http;
using Microsoft.Maui.Authentication;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
        BindingContext = this;
        Title = "Login";
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string clientId = "3MVG9FINO1nsxRuCKdhiAIOm6bjbYgBzJOWu9V7zNWfXv.W7NNd6a5zOXrIN3gVQxpS48QA0Qo6zbweC4T8lH";
        string clientSecret = "A72411D38F432952D201A224FB7C57C2BA7BE516278D87EB4FD7DC05F5C1AF65";
        string redirectUri = "myapp://oauth/callback";
        string authorizeBaseUrl = "https://login.salesforce.com/services/oauth2/authorize";
        string tokenUrl = "https://login.salesforce.com/services/oauth2/token";

        string loginUrl = $"{authorizeBaseUrl}?client_id={Uri.EscapeDataString(clientId)}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          "&response_type=code" +
                          "&prompt=login";

        try
        {
            Console.WriteLine("Starting AuthenticateAsync...");
            WebAuthenticatorResult result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(loginUrl),
                new Uri(redirectUri));
            await Navigation.PushAsync(new MainPage());
            Console.WriteLine("AuthenticateAsync completed.");
            await DisplayAlert("Success", "Authentication completed successfully!", "OK");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("TaskCanceledException: The authentication process was canceled.");
            await DisplayAlert("Error", "TaskCanceledException: Authentication was canceled or timed out.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            await DisplayAlert("Error", $"Exception occurred: {ex.Message}", "OK");
        }
    }
}
