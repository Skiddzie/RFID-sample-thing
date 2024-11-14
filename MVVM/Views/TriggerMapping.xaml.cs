using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TriggerMapping : ContentPage
    {
        private TriggerMappingViewModel viewModel;
        public TriggerMapping()
        {
            BindingContext = viewModel = new TriggerMappingViewModel();
            InitializeComponent();
            Title = "Trigger Key Mapping";
        }
        private void ButtonApplyClicked(object sender, EventArgs e)
        {
            viewModel.ButtonApplyClicked();
        }
    }
}