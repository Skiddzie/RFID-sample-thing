

using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RelativeDistance : ContentPage
    {
        RelativeDistanceViewModel viewmodel;
        public RelativeDistance()
        {
            InitializeComponent();

            BindingContext = viewmodel = new RelativeDistanceViewModel();

            Title = "Relative Distance";
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewmodel.UpdateIn();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewmodel.UpdateOut();
        }
    }
}
