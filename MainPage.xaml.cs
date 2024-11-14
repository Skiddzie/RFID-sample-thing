using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM;
using System.Collections.ObjectModel;
using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }
        private ReaderModel rfidModel;
        public MainPage()
        {
            InitializeComponent();
            rfidModel = ReaderModel.readerModel;
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
