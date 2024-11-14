using Android.Widget;

namespace MauiRfidSample.MVVM.ViewModels
{
    class TriggerMappingViewModel : BaseViewModel
    {
        private string _selectedUpperTrigger, _selectedLowerTrigger;

        public TriggerMappingViewModel()
        {
            GetKeyLayoutType();
        }

        public string SelectedUTrigger { get => _selectedUpperTrigger; set { _selectedUpperTrigger = value; OnPropertyChanged(); } }
        public string SelectedLTrigger { get => _selectedLowerTrigger; set { _selectedLowerTrigger = value; OnPropertyChanged(); } }

        internal void ButtonApplyClicked()
        {
            if (SelectedUTrigger != null && SelectedLTrigger != null)
            {
                bool result = rfidModel.SetKeyLayoutType(SelectedUTrigger, SelectedLTrigger);
                if (result)
                {
                    Toast.MakeText(Android.App.Application.Context, "Trigger Selection applied successfully", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "Trigger selection settings not allowed", ToastLength.Short).Show();
                    GetKeyLayoutType();
                }
            }

        }

        public override void ReaderConnectionEvent(bool connection)
        {
            base.ReaderConnectionEvent(connection);
            GetKeyLayoutType();
        }

        private void GetKeyLayoutType()
        {
            SelectedUTrigger = rfidModel.GetUpperTrigger();
            SelectedLTrigger = rfidModel.GetLowerTrigger();
        }

    }
}
