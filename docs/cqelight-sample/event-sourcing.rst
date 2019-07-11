Event sourcing
==============
Au jour d'aujourd'hui, notre système n'utilise qu'une seule et même source de données pour la lecture que pour la prise de décision. On a vu précédemment qu'on a commencé à faire des choix pour optimiser la lecture. Sauf qu'en contre-partie, on optimise pas l'aspect écriture. De la même façon, les données dans notre fichier font état des choses à instant T, on ne sait pas ce qu'il s'est passé à T-1 ou depuis la genèse du système.

Pourtant notre système est déjà prêt pour ce type de fonctionnement, car chacune des actions que l'on a fait génere un ou plusieurs événements qui font état de l'histoire du système qui s'est déroulée. Il suffit de sauvegarder ces événements dans un event-store afin de pouvoir les récupérer et les rejouer si nécessaire. CQELight fournit plusieurs implémentations d'event-store pour accomplir cet objectif.

Dans le cas présent, on va prendre le plus simple à mettre en place : un event-store avec Entity Framework Core et SQLite, mais le fonctionnement global reste similaire peu importe le provider choisi ::    

    ew Bootstrapper()
       .UseInMemoryEventBus()
       .UseInMemoryCommandBus()
       .UseAutofacAsIoC(c =>
       {
       })
       .UseEFCoreAsEventStore(
       new CQELight.EventStore.EFCore.EFEventStoreOptions(
           c => c.UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(Program).Assembly.GetName).Name))))
       .Bootstrapp();
 
Il est dès lors nécessaire de générer la migration EntityFramework Core. Pour ce faire, il faut créer une classe qui permet de définir la façon d'obtenir un contexte au moment du design ::

    public class EventStoreDbContextCreator : IDesignTimeDbContextFactory<EventStoreDbContext>
    {
        public EventStoreDbContext CreateDbContext(string[] args)
        {
            return new EventStoreDbContext(new DbContextOptionsBuilder<EventStoreDbContext>()
                        .UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeofEventStoreDbContextCreator).Assembly.GetName().Name))
                        .Options, SnapshotEventsArchiveBehavior.Delete);
        }
    }

Cette classe sera utilisé par le CLI d'EF Core afin d'avoir accès au contexte pour générer la migration. Dès lors, il suffit d'appeler le CLI pour générer une migration : dotnet ef migrations add EventStoreMigration -c EventStoreDbContext. La migration sera générée dans le projet.

.. note:: Il n'est pas nécessaire de faire appel au CLI pour demander un database update ou de faire une migration par code, le bootstrapper se charger de récupérer la migration et de l'appliquer si vous précisez bien l'option MigrationsAssembly dans la connectionString.

.. note:: Ces blocs de code ne sont donnés que pour l'exemple. Dans un environnement de production, il est préférable de stocker la chaine de connexion et le paramétrage à un endroit plus sécurisé, comme une variable d'environnement ou un fichier de configuration.

En ayant suivant les étapes précédentes, on arrive donc à avoir un système fonctionnel avec un event-store, qui capte et enregistre les événéments précédemment créés. Le souci, c'est que l'état actuel des événements ne permettent pas de remettre le système dans une condition fonctionnelle, car les événements ne sont pas liés à une identité d'aggrégat. Il faut modifier légérement l'événement de création :

::

    public sealed class FamilleCreee : BaseDomainEvent
    {
    
        public NomFamille NomFamille { get; private set; }
    
        private FamilleCreee() { }
    
        internal FamilleCreee(NomFamille nomFamille)
        {
            NomFamille = nomFamille;
            AggregateId = nomFamille;
            AggregateType = typeof(Famille);
        }
    
    }

On rajoute dans l'événement ``FamilleCreee`` le type et l'id de l'agrégat pour que ce dernier puisse récupérer les événements le concernant.

.. note:: Cette modification n'est nécessaire que pour les événements qui ne sont pas enregistrés et publiés depuis une instance d'aggrégat, car si c'est le cas, le framework CQELight est capable de renseigner ces informations automatiquement, comme lors de l'action ``AjouterPersonne``.

Dès que cette modification est appliquée, si on regarde la BDD, on constate qu'une ligne est ajoutée dans la table Event chaque fois qu'un événement est publié. Ces événéments consitue la base en écriture dans un modèle CQRS, et doivent être utilisé par notre aggrégat chaque fois qu'une action est demandée. Il faut donc modifier notre agrgégat pour le transformer en aggrégat "event-sourcé", et ceci en deux actions :

- Implémenter l'interface ``IEventSourcedAggregate`` (dans le namespace ``CQELight.Abstractions.EventStore.Interfaces``)
- Aggrémenter l'objet ``FamilleState`` de handlers capable de gérer les événements étant arrivés

Gestion de l'état de l'aggrégat
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

En modifiant notre ``AggregateState``, on va lui ajouter la possibilités de savoir comment réagir aux événement pour gérer la réhydratation depuis la base, par le biais de la méthode ``AddHandler``. Cette méthode défini l'extraction des informations depuis les événements vers la classe d'état elle-même. Etant donné que l'on agit en réhydratation, on s'assure que l'état reste cohérent, en définissant les setters privés : 

::

    class Famille : AggregateRoot<NomFamille>
    {
        private FamilleState _state;
    
        private class FamilleState : AggregateState
        {
    
            public List<Personne> Personnes { get; private set; }
            public string Nom { get; private set; }
    
            public FamilleState()
            {
                Personnes = new List<Personne>();
                AddHandler<FamilleCreee>(FamilleCree);
            }
    
            private void FamilleCree(FamilleCreee obj)
            {
                Nom = obj.NomFamille.Value;
                _nomFamilles.Add(obj.NomFamille);
    
            }
        }
        [...]
    }

Gestion d'un aggrégat event-sourcé
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Il est nécessaire d'implémenter l'interface ``IEventSourcedAggregate`` afin de donner au système la visibilité sur les possibilités de cet aggrégat. L'implémentation de cette interface nécessite de redéfinir le comportement RehydrateState, qui permet à l'event-store de réhydrater l'aggrégat simplement.

.. note:: Il est possible d'hériter d'une classe qui permet cela de façon automatique : ``EventSourcedAggregate``. Cette classe nécessite cependant de préciser le type de state que l'aggrégat va gérer, impliquant le fait que cette classe doit avoir une visibilité publique. La difficulté va résider dans la vigilance nécessaire pour consever le périmètre de la responsabilité de cette classe, à savoir conserver la cohésion des données.

::

    class Famille : AggregateRoot<NomFamille>, IEventSourcedAggregate
    {
        
            public void RehydrateState(IEnumerable<IDomainEvent> events)
            {
                _state.ApplyRange(events);
                Id = _state.Nom;
            }
        
        [...]
    }

Assemblage des élements
^^^^^^^^^^^^^^^^^^^^^^^

La problématique de réhydratation est une problématique infrastructurale, et revient donc aux command handlers. Afin d'illustrer un cas, on va l'appliquer sur la gestion de la command AjouterPersonne. Cela va se concrétiser en demandant une injection d'un IAggregateEventStore, afin de pouvoir récupérer une aggrégat totalement réhydraté ::

    class AjouterPersonneCommandHandler : ICommandHandler<AjouterPersonneCommand>, IAutoRegisterType
    {
        private readonly IAggregateEventStore _eventStore;
    
        public AjouterPersonneCommandHandler(IAggregateEventStore eventStore)
        {
            _eventStore = eventStore;
        }
        public async Task<Result> HandleAsync(AjouterPersonneCommand command, ICommandContext context = null)
        {
            var famille = await _eventStore.GetRehydratedAggregateAsync<Famille>(command.NomFamille);
            famille.AjouterPersonne(command.Prenom, new InfosNaissance(command.LieuNaissance, command.DateNaissance));
            await famille.PublishDomainEventsAsync();
            return Result.Ok();
        }
    }

La boucle est bouclée, on gère le circuit des événements depuis l'envoi jusqu'à l'utilisation pour la réhydratation. En utilisant la réhydration pour effectuer une commande, on s'assure d'utiliser le fil de l'histoire pour prendre la meilleure décision possible, et pas uniquement un état à un instant T. Il nous reste maintenant à sécuriser notre code à l'aide de tests automatisés.