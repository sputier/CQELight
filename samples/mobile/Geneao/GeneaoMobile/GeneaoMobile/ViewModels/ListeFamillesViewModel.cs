using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using GeneaoMobile.Models;
using GeneaoMobile.Views;
using CQELight.IoC;
using Geneao.Queries;
using CQELight.Dispatcher;
using Geneao.Common.Commands;
using CQELight.Abstractions.Events.Interfaces;
using Geneao.Events;
using CQELight.Abstractions.DDD;

namespace GeneaoMobile.ViewModels
{
    public class ListeFamillesViewModel :
        BaseViewModel,
        IDomainEventHandler<FamilleCreee>
    {
        public ObservableCollection<Famille> Familles { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command AjouterFamilleCommand { get; set; }
        public Command ShowDetailsCommand { get; set; }

        private Famille _selectedFamille;
        public Famille SelectedFamille
        {
            get => _selectedFamille;
            set
            {
                Set(ref _selectedFamille, value);
                ShowDetailsCommand.ChangeCanExecute();
            }
        }
        
        private string _nouveauNom;
        public string NouveauNom
        {
            get => _nouveauNom;
            set => Set(ref _nouveauNom, value);
        }

        public ListeFamillesViewModel()
        {
            Title = "Browse";
            Familles = new ObservableCollection<Famille>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AjouterFamilleCommand = new Command(async () =>
            {
                var result = await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand(NouveauNom));
                if (!result)
                {

                }
            });
            ShowDetailsCommand = new Command(async () => { }, () => SelectedFamille != null);
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Familles.Clear();
                using (var scope = DIManager.BeginScope())
                {
                    var query = scope.Resolve<IRecupererListeFamille>();
                    var items = await query.ExecuteQueryAsync();
                    foreach (var item in items)
                    {
                        Familles.Add(new Famille
                        {
                            Nom = item.Nom
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Task<Result> HandleAsync(FamilleCreee domainEvent, IEventContext context = null)
        {
            Familles.Add(new Famille
            {
                Nom = domainEvent.NomFamille.Value
            });
            NouveauNom = string.Empty;
            return Result.Ok();
        }
    }
}