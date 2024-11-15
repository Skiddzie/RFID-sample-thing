using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using MauiRfidSample.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;



namespace MauiRfidSample.MVVM.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AssignmentPage : ContentPage
    {
        private AssignmentPageModel viewmodel;

        public AssignmentPage()
        {
            InitializeComponent();
            BindingContext = viewmodel = new AssignmentPageModel();
            Title = "Item inventory";
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