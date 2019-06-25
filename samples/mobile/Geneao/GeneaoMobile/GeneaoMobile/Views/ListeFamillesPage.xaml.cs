using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using GeneaoMobile.Models;
using GeneaoMobile.Views;
using GeneaoMobile.ViewModels;

namespace GeneaoMobile.Views
{
    [DesignTimeVisible(false)]
    public partial class ListeFamillesPage : ContentPage
    {
        ListeFamillesViewModel viewModel;

        public ListeFamillesPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ListeFamillesViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Familles.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }

        private async void FamilleDetailsToolbar_Clicked(object sender, EventArgs e)
        {
            if (viewModel.SelectedFamille != null)
            {
                await Navigation.PushAsync(new FamilleDetailsPage(new FamilleDetailsViewModel(viewModel.SelectedFamille)));
            }
        }
    }
}