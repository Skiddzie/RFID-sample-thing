using System.Diagnostics;

namespace AscentSolutions.ZebraRfid
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
