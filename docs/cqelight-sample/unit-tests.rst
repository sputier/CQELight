Tests unitaires et fonctionnels
===============================
.. note:: Chez Hybrid Technologies Solutions, nous privilégions les développements qui suivent la logique TDD (Test Driven Development), qui consiste à écrire les tests avant l'implémentation. Cette logique ayant fait ses preuves, elle n'est plus à démontrer. Cependant, dans le cadre de cet exercice, les tests arrivent en fin de course, car l'objectif n'est pas d'apprendre le TDD (bien que si vous le désirez, Hybrid Technologies Solutions peut vous former à cela) mais bien de découvrir les possibilités de CQELight. Les tests arrivent donc à cette étape uniquement pour cette raison, et aucune autre.

Comme nous l'avons vu sur la page concernant les tests unitaires et d'intégration, CQELight fournit un package permettant d'écrire des tests automatisés et de vérifier la logique métier. Dans le cadre de notre exemple, nous allons écrire deux tests, un unitaire et un d'intégration, pour démontrer le principe.

La totalité du code ne sera pas couverte, mais il ne tient qu'à vous de poursuivre l'exercice et de tenter d'en couvrir un maximum. A noter cependant qu'il est pertinent de focaliser les efforts d'écriture et de maintenance des tests sur la partie métier de votre application et non la partie technique, celle-ci reposant généralement sur des outils étant déjà testés et benchmarkés.

Test unitaire
^^^^^^^^^^^^^
Afin d'écrire un test pertinent sur notre exemple, nous allons vérifier le comportement de la méthode AjouterPersonne de l'aggrégat Famille, s'assurer de la récupération d'un événement ou d'un résultat négatif au sens métier. Il y a deux possibilités pour tester ce comportement : soit directement auprès de l'aggrégat, soit auprès d'un command handler.

Dans le contexte d'un test unitaire, qui doit être détaché de toute problématique infrastructurale et technique, il est pertinent de faire cet appel sur l'aggrégat lui-même et de vérifier le résultat, et ce pour deux raisons:

1. Cela permet d'écrire un test qui s'exécutera très rapidement et ne concernera que la logique métier
2. On embarque pas dans notre test des notions de mock pour simuler certains comportement techniques particuliers, on reste au niveau d'une fonction mathématique pure

En faisant cela, non seulement on sécurire notre logique métier, mais on fourni également une documentation implicite pour les autres développeurs (ils voient un exemple concret d'appel). En évitant de descendre trop bas dans l'implémentation, on s'assure également de garder une API utilisable. Dans le cas présent, l'existance d'un TU sur l'API AjouterPersonne permet le refactoring au sein de l'implémentation, mais "bloque" la signature de la méthode, qui peut être utilisée ailleurs.

.. note:: Cela amène également le sujet de l'importance de conserver des API rétro-compatibles si vous n'avez pas la main sur l'ensemble des services utilisant vos APIs. Avoir un ou plusieurs tests qui couvrent ces API permettra d'éviter de casser une API utilisée par d'autres services. La notion de Command est là pour abstraire ce problème et permettre à votre domaine d'évoluer sans impact, d'où il est important d'utiliser ce système plutôt que de faire appel directement à l'aggrégat, dans la mesure du possible.

La première étape consiste à créer un projet de tests automatisés. Nous utilisons xUnit pour cela, mais vous êtes libre d'utiliser le framework de votre choix. A ce projet, nous allons ajouter le package CQELight.TestFramework. Notre classe doit hériter de BaseUnitTestClass pour profiter de la panoplie d'outils à disposition. Ecrivons notre premier test qui couvre le cas optimal fonctionnel (ajout avec succès d'une personne qui n'existe pas dans la famille).
::

    public class FamilleTests : BaseUnitTestClass
    {
        [Fact]
        public void Ajouter_Personne_Should_Create_Event_PersonneAjoute()
        {
            var familleResult = Famille.CreerFamille("UnitTest");
            familleResult.Should().BeOfType<Result<NomFamille>>();
    
            var famille = new Famille("UnitTest");
    
            var result = famille.AjouterPersonne("First",
                new InfosNaissance("Paris", new DateTime(1965, 12, 03)));
    
            result.IsSuccess.Should().BeTrue();
            famille.DomainEvents.Should().HaveCount(1);
            famille.DomainEvents.First().Should().BeOfType<PersonneAjoutee>();
            var evt = famille.DomainEvents.First().As<PersonneAjoutee>();
            evt.Prenom.Should().Be("First");
            evt.DateNaissance.Should().BeSameDateAs(new DateTime(1965, 12, 03));
            evt.LieuNaissance.Should().Be("Paris");
    
        }
    }
 
Ce test suit une logique simple : création d'une famille dans le système, récupération de l'aggrégat de famille, ajout d'une personne, vérification que tout est ok. Notre premier test est écrit. Il manque maintenant les tests "négatifs". Un exemple de ceux-ci peut être ::

    [Fact]
    public void Ajouter_Personne_Already_Exists_Should_Returns_Result_Fail()
    {
        var familleResult = Famille.CreerFamille("UnitTest");
        familleResult.Should().BeOfType<Result<NomFamille>>();
    
        var famille = new Famille("UnitTest");
    
        famille.AjouterPersonne("First",
            new InfosNaissance("Paris", new DateTime(1965, 12, 03)));
    
        var result = famille.AjouterPersonne("First",
            new InfosNaissance("Paris", new DateTime(1965, 12, 03)));
    
        result.IsSuccess.Should().BeFalse();
        result.Should().BeOfType<Result<PersonneNonAjouteeCar>>();
    
        var raison = result.As<Result<PersonneNonAjouteeCar>>().Value;
        raison.Should().Be(PersonneNonAjouteeCar.PersonneExistante);
    }

Lors de l'exécution des tests, un des deux tests ne passent pas, car en effet, la famille existe déjà dans la variable statique ``_nomFamilles`` (stockée dans l'aggrégat ``Famille``) ! Pour palier à ce problème, nous avons plusieurs solutions. Une d'entre elle consisterait à exposer les variables de portée internal à notre assembly de tests. Une autre consisterait à utiliser un autre nom de famille. Pour résoudre vite ce problème, nous allons déplacer la création de famille dans une méthode d'initialisation de notre constructeur ::

    private static bool s_Init = false;
    public FamilleTests()
    {
        if(!s_Init)
        {
            Famille.CreerFamille("UnitTest");
            s_Init = true;
        }
    }

Nos deux tests passent avec succès. Maintenant que vous avez la logique, il devient très facile d'écrire les tests pour le cas PrenomInvalide. Attention à un point dans ce cas précis : si les informations de naissance sont mal renseignées, le test échoue mais pas à cause d'une logique implémentée dans l'aggrégat mais dans l'entité personne. Le choix vous appartient d'écrire le test au niveau entité ou aggrégat, il faut juste garder à l'esprit de ne pas bloquer le refactoring en descendant trop bas.

Test d'intégration
^^^^^^^^^^^^^^^^^^
Dans une logique de test d'intégration, il convient de mettre en place la structure pour s'assurer que la totalité des élements s'assemblent bien. Nous allons tester la même chose que précédemment, mais en mode intégration. Il est nécessaire de créer un nouveau projet de tests automatisés afin d'y implémenter notre test d'intégration, en suivant la convention de nommage : xxxx.Integration.Tests. Cette règle a été définie afin de clairement séparer les tests unitaires (répétables et intégrables dans un pipeline devops) des tests d'intégrations (lancement moins fréquent et dépendant de contraintes d'environnement rendant l'automatisation moins évidente).

Cette règle peut-être contournée, mais il est préférable de suivre la recommandation pour profiter au maximum des optimisations prévues pour chaque type de test ::

    [Fact]
    public async Task Ajouter_Personne_Should_Publish_Event_PersonneAjoute()
    {
        new Bootstrapper()
            .UseInMemoryEventBus()
            .UseInMemoryCommandBus()
            .UseAutofacAsIoC(_ => { })
            .UseEFCoreAsEventStore(
                new CQELight.EventStore.EFCore.EFEventStoreOptions(
                    c => c.UseSqlite("FileName=events_tests.db", opts => opts.MigrationsAssembly(typeof(FamilleIntegrationTests).Assembly.GetName).Name)),
                    archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
            .Bootstrapp();
    
        await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand("UnitTest"));
    
        var command = new AjouterPersonneCommand("UnitTest", "First", "Paris", new DateTime(1965, 12, 03));
    
        var evt = await Test.WhenAsync(() => CoreDispatcher.DispatchCommandAsync(command))
            .ThenEventShouldBeRaised();
    
        evt.Prenom.Should().Be("First");
        evt.DateNaissance.Should().BeSameDateAs(new DateTime(1965, 12, 03));
        evt.LieuNaissance.Should().Be("Paris");
    
    }

.. note:: Il est nécessaire de copier ou d'ajouter les migrations pour l'event store dans le projet pour que l'intégration puisse se faire de part en part. On constate la présence du bootstrapper (nécessaire pour mettre en place l'infrastructure) et l'utilisation du framework de test (avec la méthode ``Test.WhenAsync``).

Comme cela se remarque facilement, le test d'intégration est plus lourd à mettre en place et plus long à l'exécution, c'est pourquoi il est recommandé de prioriser les tests unitaires lorsqu'il convient de tester le métier. Cependant, il n'en reste pas moins intéressant d'en avoir quelques-uns pour sécuriser ce qui peut être automisé.

Vous avez dorénavant la possibilité d'écrire des tests pour votre code métier !