using System.Diagnostics;

namespace MauiRfidSample
{
    public partial class App : Application
    {
        public static AscentWebService SharedAscentWebService { get; } = new AscentWebService();
        public static Items SharedItemsService { get; } = new Items();
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
