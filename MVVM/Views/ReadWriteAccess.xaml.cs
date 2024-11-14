
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using System;


namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReadWriteAccess : ContentPage
    {
        ReadWriteOperationsModel viewmodel;
        public ReadWriteAccess()
        {
            InitializeComponent();
            Title = "Item Commission";
            BindingContext = viewmodel = new ReadWriteOperationsModel();
        }

        private void ReadOperation(object sender, EventArgs e)
        {
            if (tagPattern.Text.Equals(""))
                DisplayAlert("Invalid Parameter", "Empty TAG PATTERN", "Cancel");
            else
                viewmodel.AccessOperationsReadClicked();
        }

        private void WriteOperation(object sender, EventArgs e)
        {
            if (tagPattern.Text.Equals("") || data.Text == null || (data.Text != null && data.Text.Equals("")))
                DisplayAlert("Invalid Parameter", "Empty TAG PATTERN or data", "Cancel");
            else
                viewmodel.AccessOperationsWriteClicked();
        }

        private void LockOperation(object sender, EventArgs e)
        {
            if (tagPattern.Text.Equals(""))
                DisplayAlert("Invalid Parameter", "Empty TAG PATTERN", "Cancel");
            else
                viewmodel.AccessOperationsLockClicked();
        }

        private void ButtonReadWrite(object sender, EventArgs e)
        {
            lockView.IsVisible = false;
            readWriteView.IsVisible = true;
            buttonReadWrite.BackgroundColor = Color.FromRgb(120, 90, 221);
            buttonLock.BackgroundColor = Color.FromRgb(172, 153, 234);
        }

        private void ButtonLock(object sender, EventArgs e)
        {
            lockView.IsVisible = true;
            readWriteView.IsVisible = false;
            buttonReadWrite.BackgroundColor = Color.FromRgb(172, 153, 234);
            buttonLock.BackgroundColor = Color.FromRgb(120, 90, 221);
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