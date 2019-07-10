Event sourcing avec EF Core
===========================
Il est possible d'utiliser Entity Framework Core, et donc de profiter de la flexibilité de provider qu'il intègre, pour faire un système event-sourcé. Les événements seront persistés dans une base de données relationnelle. Bien que cela ne soit pas sa fonctionnalité initiale, cela permet d'avoir un système fonctionnel rapidement et simplement. 

Etant donné que les bases de données relationnelle utilisent un schéma pour la persistance des données, il est nécessaire de créer une migration EF Core dans votre projet. Il faut en premier lieu définir dans le projet pour qu'il sache comment configurer le contexte : 

::

    // With SQLite
    public class EventStoreDbContextCreator : IDesignTimeDbContextFactory<EventStoreDbContext>
    {
        public EventStoreDbContext CreateDbContext(string[] args)
        {
            return new EventStoreDbContext(new DbContextOptionsBuilder<EventStoreDbContext>()
                        .UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(EventStoreDbContextCreator).Assembly.GetName().Name))
                        .Options, SnapshotEventsArchiveBehavior.Delete);
        }
    }

Une fois ceci fait, il est nécessaire de créer une migration EF Core. Pour ce faire, il faut lancer la commande suivante sur votre projet exécutable : 

::

    // Dotnet CLI
    dotnet ef migrations add EventStoreMigration -c EventStoreDbContext
    // VS 
    Add-Migration EventStoreMigration -c EventStoreDbContext

La migration est ajoutée à votre projet. La dernière étape pour utiliser EF Core comme EventStore et de le déclarer dans le bootstrapper : 

::

    // With SQLite
    new Bootstrapper()
                .UseEFCoreAsEventStore(
                new CQELight.EventStore.EFCore.EFEventStoreOptions(
                    c => c.UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)),
                    archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
                .Bootstrapp();

Le code de configuration du le ``DbContextionOptionsBuilder`` peut être mutualisé afin d'être écrit une seule fois. 
Le contexte n'est pas ajouté dans le container IoC d'un projet Asp.Net Core, comme on pourrait le faire avec ``services.AddDbContext``. Ceci est du au fait qu'il est déconseillé d'utiliser directement le contexte pour accéder à l'event-store, et qu'il est recommandé d'utiliser les interfaces ``IAggregateEventStore`` et ``IEventStore`` (ou ``IReadEventStore`` et ``IWriteEventStore``), car beaucoup de règles de gestion sont implémentées dedans et ne sont pas disponibles au niveau du contexte EF. Utiliser le contexte EF directement pourrait compromettre l'intégrité de votre EventStore, surtout en écriture. 

Spécificités
^^^^^^^^^^^^
Le provider EF Core dispose de certaines spécificités permettant d'optimiser le traitement avec la base relationnelle. La classe ``EFEventStoreOptions`` permet de préciser chacun de ces spécificités. 

- ``SnapshotBehaviorProvider`` et ``ArchiveBehavior`` permettent de précisier le mode de fonctionnement du moteur de snapshot. Pour plus de renseignements sur la notion de snapshot, voir la page sur l'event sourcing. 
- ``DbContextOptions`` définit le mode d'accès à la base principale des événements 
- ``ArchiveDbContextOptions`` définit le mode d'accès à la base d'archive des événements. Note : cette propriété est obligatoire si la valeur du membre ArchiveBehavior est définie à ``StoreToNewDatabase``
- ``BufferInfo`` permet de définir le comportement du tampon utilisé pour optimiser les requêtes vers le SGDB.

La notion de buffer a été ajoutée pour éviter de faire trop d'appels à la base dans le cadre d'un système très sollicité par l'envoi d'événements unitaires. Par exemple, si on imagine un système qui propage un événement toutes les 200 millisecondes, on risque de se retrouver avec utilisation intensive d'EF Core et du système transactionnel qui va ralentir notre event-store. Les membres suivants sont disponibles :

- ``UseBuffer`` : flag d'activation
- ``AbsoluteTimeOut`` : Timeout absolu à partir duquel les événements doivent être persistés obligatoirement
- ``SlidingTimeOut`` : Timeout glissant permettant de définir une durée de persistance à partir de laquelle les événements doivent être persistés s'il n'y en a pas de nouveaux

Deux configurations sont disponibles par le biais de variables statiques globales : ``BufferInfo.Enabled`` et ``BufferInfo.Disabled``. L'utilisation de la valeur Enabled utilisera une valeur de 10 secondes en timeout absolu et 2 secondes en timeout glissant. 

Il faut être vigilant avec l'utilisation du buffer, car s'il est améliore effectivement les performances sur les systèmes qui sont souvent sollicités en terme de propagation d'événements, il va ralentir un système qui ne propage pas énormément d'event. Pour savoir s'il vous faut l'utiliser, faites une statistique du temps moyen de propagation d'événement dans votre système et voyez si ce temps moyen est inférieur à 2 secondes. Si oui, considérez l'utilisation du buffer. Si non, ne l'activez pas (par défaut). 