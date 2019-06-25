using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CQELight.Abstractions.DDD;
using CQELight.Abstractions.Events.Interfaces;
using CQELight.Dispatcher;
using CQELight.IoC;
using Geneao.Common.Commands;
using Geneao.Common.Queries;
using Geneao.Events;
using GeneaoMobile.Models;
using Xamarin.Forms;

namespace GeneaoMobile.ViewModels
{
    public class FamilleDetailsViewModel :
        BaseViewModel,
        IDomainEventHandler<PersonneAjoutee>
    {
        public Famille Famille { get; set; }

        public ObservableCollection<PersonneInfos> Personnes { get; set; }
            = new ObservableCollection<PersonneInfos>();

        public Command LoadPersonnesCommand { get; set; }
        public Command SaveNewPersonneCommand { get; set; }

        private string _nouveauPrenom;
        public string NouveauPrenom
        {
            get => _nouveauPrenom;
            set
            {
                Set(ref _nouveauPrenom, value);
                SaveNewPersonneCommand.ChangeCanExecute();
            }
        }

        private string _nouveauLieuNaissance;
        public string NouveauLieuNaissance
        {
            get => _nouveauLieuNaissance;
            set
            {
                Set(ref _nouveauLieuNaissance, value);
                SaveNewPersonneCommand.ChangeCanExecute();
            }
        }

        private DateTime? _nouvelleDateNaissance;
        public DateTime? NouvelleDateNaissance
        {
            get => _nouvelleDateNaissance;
            set
            {
                Set(ref _nouvelleDateNaissance, value);
                SaveNewPersonneCommand.ChangeCanExecute();
            }
        }


        public FamilleDetailsViewModel(Famille famille)
        {
            Title = famille.Nom;
            Famille = famille;
            LoadPersonnesCommand = new Command(async () =>
            {
                using (var scope = DIManager.BeginScope())
                {
                    var query = scope.Resolve<IRecupererListePersonnes>();
                    var result = await query.ExecuteQueryAsync(new Geneao.Common.Identity.NomFamille(famille.Nom));
                    foreach (var item in result)
                    {
                        Personnes.Add(new PersonneInfos
                        {
                            Prenom = item.Prenom,
                            DateNaissance = item.DateNaissance,
                            LieuNaissance = item.LieuNaissance
                        });
                    }
                }
            });
            SaveNewPersonneCommand = new Command(async () =>
            {
                var result = await CoreDispatcher.DispatchCommandAsync(
                    new AjouterPersonneCommand(
                        new Geneao.Common.Identity.NomFamille(famille.Nom),
                        NouveauPrenom,
                        NouveauLieuNaissance,
                        NouvelleDateNaissance.Value));
                if (!result)
                {
                    //TODO show error message
                }
                else
                {
                    NouveauPrenom = "";
                    NouveauLieuNaissance = "";
                    NouvelleDateNaissance = null;
                }
            }, () => !string.IsNullOrWhiteSpace(NouveauPrenom) && !string.IsNullOrWhiteSpace(NouveauLieuNaissance) && NouvelleDateNaissance.HasValue);
        }

        public Task<Result> HandleAsync(PersonneAjoutee domainEvent, IEventContext context = null)
        {
            if (domainEvent.NomFamille.Value == Famille.Nom)
            {
                Personnes.Add(new PersonneInfos
                {
                    Prenom = domainEvent.Prenom,
                    DateNaissance = domainEvent.DateNaissance,
                    LieuNaissance = domainEvent.LieuNaissance
                });
            }
            return Result.Ok();
        }
    }
}
