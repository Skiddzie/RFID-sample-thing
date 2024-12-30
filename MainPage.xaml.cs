using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM;
using System.Collections.ObjectModel;
using MauiRfidSample.MVVM.ViewModels;
using System.Diagnostics;
namespace MauiRfidSample
{
    public partial class MainPage : ContentPage
    {
        private AscentWebService _webService;
        public ObservableCollection<string> Items { get; set; }
        private ReaderModel rfidModel;
        public MainPage()
        {
            InitializeComponent();
            _webService = App.SharedAscentWebService;
            InitializePageAsync();
        }

        private async void InitializePageAsync()
        {
            rfidModel = ReaderModel.readerModel;
            //bool success = SecureStorage.Default.Remove("AccessToken");

            string? accessTokenString = await SecureStorage.GetAsync("AccessToken");
            string? instanceUrl = await SecureStorage.GetAsync("InstanceUrl");

            _webService.SetOAuthToken(accessTokenString, instanceUrl);

            if (string.IsNullOrEmpty(accessTokenString))
            {
                Trace.WriteLine("Access Token is null or empty. Navigating to Login page...");
                await Navigation.PushAsync(new Login());
            }
            else
            {
                Trace.WriteLine("Access Token found:");
                Trace.WriteLine(accessTokenString);
                Trace.WriteLine("Instance URL:");
                Trace.WriteLine(instanceUrl);
            }
        }

        private async void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs args)
        {

            (sender as ListView).SelectedItem = null;

            if (args.SelectedItem != null)
            {
                PageDataViewModel pageData = args.SelectedItem as PageDataViewModel;
                Page page = (Page)Activator.CreateInstance(pageData.Type);
                await Navigation.PushAsync(page, false);
            }
        }

        internal void OnResume()
        {
            rfidModel?.SetTriggerMode();
            Console.WriteLine("OnResume");
        }

        internal void OnSleep()
        {
            //rfidModel?.Disconnect();
            Console.WriteLine("OnSleep");
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MVVM.Views.About());
        }
    }

}
