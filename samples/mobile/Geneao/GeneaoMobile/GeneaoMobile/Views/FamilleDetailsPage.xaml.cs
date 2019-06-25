using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using GeneaoMobile.Models;
using GeneaoMobile.ViewModels;

namespace GeneaoMobile.Views
{
    [DesignTimeVisible(false)]
    public partial class FamilleDetailsPage : ContentPage
    {
        FamilleDetailsViewModel viewModel;

        public FamilleDetailsPage(FamilleDetailsViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Personnes.Count == 0)
                viewModel.LoadPersonnesCommand.Execute(null);
        }
    }
}