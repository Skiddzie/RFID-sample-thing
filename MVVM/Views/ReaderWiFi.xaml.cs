using MauiRfidSample.MVVM.ViewModels;



namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReaderWiFi : ContentPage
    {
        private ReaderWiFiViewModel viewModel;
        public ReaderWiFi()
        {
            BindingContext = viewModel = new ReaderWiFiViewModel();
            InitializeComponent();
            Title = "WiFi Settings";
        }

        private void RefreshClicked(object sender, EventArgs e)
        {
            viewModel.Refresh();
        }

        private void OnItemTappedAvilableProfiles(object sender, ItemTappedEventArgs e)
        {
            viewModel.SelectedAvilableProfile(e);
        }

        private void OnItemTappedSavedProfiles(object sender, ItemTappedEventArgs e)
        {
            viewModel.SelectedSavedProfile(e);
        }

        private void ProtocolPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            viewModel.ProtocolPickerSelectedIndexChanged(sender);
        }

        private void EAPPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            viewModel.EAPPickerSelectedIndexChanged(sender);
        }

        private void AddProfileClicked(object sender, EventArgs e)
        {
            viewModel.AddProflie();
        }

        private void OnTappedConnectedProfile(object sender, EventArgs e)
        {
            viewModel.ConnectedProfileClicked();
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            viewModel.ClosePopup();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.UpdateReaderWiFiEventsIn();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.UpdateReaderWiFiEventsOut();
        }
    }
}