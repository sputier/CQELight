Tests unitaires et fonctionnels
===============================

Chez Hybrid Technologies Solutions, nous nous efforçons de développer avec des tests unitaires et fonctionnels. A cet effet, il nous était impensable d'imaginer que si d'un côté, nous réalisons un framework pour aider les développeurs à se focaliser sur le métier, de l'autre nous ne prévoyons pas la possibilité de facilement tester le code écrit avec notre framework.

Pour répondre à ce besoin, nous avons créé un package à ajouter à vos projets de tests unitaires : **CQELight.TestFramework**. Ce package contient un certain nombre d'outils que nous avons jugé utile de rajouter pour vous aider à faire vos tests unitaires.

..note :: A la différence de l'ensemble du framework, nous avons ajouté certains packages que nous utilisons quotidiennement en test unitaire, à savoir `Moq <https://github.com/moq/moq4>`_ et `FluentAssertions <https://fluentassertions.com/>`_. Nous n'avons pas trouvé cela problématique considérant la popularité de ces packages. Cependant, si plus tard, cela poserait problème, nous créerons des "sous-packages" pour ces points précis. De fait, en installant notre TestFramework, vous installerez automatiquement ces deux packages également, et vous profiterez de certaines méthodes d'extension que nous avons réalisées sur ces derniers.

BaseUnitTestClass
^^^^^^^^^^^^^^^^^

La première chose que nous mettons à votre disposition est une classe de base pour vos tests unitaires, ``CQELight.TestFramework.BaseUnitTestClass``. Cette classe effectue quelques actions automatiquement à la construction.

La classe ``CQELight.TestFramework.UnitTestTools`` vous fournit deux flags qui permettent de détecter le mode de fonctionnement de votre classe de test. Ces flags peuvent être utile à n'importe quelle partie pour savoir le contexte.

- Le flag ``IsInUnitTestMode`` est à vrai dès lors qu'une instance a été construite.
- Le flag ``IsInIntegrationTestMode`` se mets à vrai si le nom de votre projet contient .Integration.

Ces flags sont automatiquement déterminé dans la construction de la classe de base ``BaseUnitTestClass``.

.. note:: Il s'agit ici d'une convention que nous avons adoptée, permettant de distinguer les tests unitaires des tests d'intégration en se basant sur le nom du projet. Cela peut s'avérer utile pour vos tests nécessitant un contexte particulier (accès au système de fichier, base de données, connexion réseau, ...) et qui ne peuvent pas être exécutés n'importe où.

Afin de vous permettre d'utiliser l'IoC facilement, nous avons développé une couche factice que vous pouvez alimenter selon les besoins de vos tests. Le constructeur de la classe ``BaseUnitTestClass`` créé une instance de notre scope factory de test et initialise le système d'IoC avec celle-ci (hors tests d'intégration). Si vous désirez que ce comportement ne soit pas exécuté, vous pouvez préciser le paramètre constructeur, ``disableIoC``, à true pour empêche cette initialisation. Une fois celle-ci faite, vous aurez accès au membre protégé ``_testFactory`` dans lequel vous pouvez ajouter les enregistrements que vous avez besoin pour vos tests (en ajoutant une ou plusieurs valeurs dans la propriété Instances qui alimenteront automatiquement vos scope).

Exemple ::

    public class MyUnitTests : BaseUnitTestClass
    {
        public MyUnitTests()
        {
            var myAbstractionMock = new Mock<IMyAbstraction>();
            _testScopeFactory.Instances.Add(typeof(IMyAbstraction), myAbstractionMock.Object);
        }
    }

.. note:: Il s'agit ici d'une implémentation factice destinée à simplifier les tests unitaires et elle ne saurait en aucun cas se substituer à un vrai système d'IoC ni prétendre en avoir les mêmes possibilités (injection automatique d'abstractions dans le constructeur, injection dans les propriétés, ...). Pensez à conserver la vision test unitaire pour que cela soit adapté. Si vous besoins sont plus complexe, il s'agit probablement d'un test d'intégration, et dans ce cas précis, il est recommandé d'utiliser un vrai container IoC.

Finalement, il est possible d'ajouter un enregistrement dans ce scope factice en dehors du constructeur, à l'aide de la méthode protégée ``AddRegistrationFor``. Cela vous permet d'ajouter une implémentation dans votre test directement juste avant l'exécution ::

    [Fact]
    public void MyUnitTest()
    {
        var myAbstractionMock = new Mock();
        AddRegistrationFor(myAbstractionMock.Object);
    }

La méthode ``CleanRegistrationInDispatcher`` est un raccourci qui permet de vider le container statique du ``CoreDispatcher`` qui contient les instances enregistrées directement dedans (de type ``IMessageHandler``, ``IDomainEventHandler`` et ``ICommandHandler``).

IoC
^^^

Comme nous l'avons vu précédemment, nous mettons à disposition un ``TestScope`` et un ``TestScopeFactory``. Ces deux classes vous permettront de simuler le comportement du container IoC au niveau méthode. Le ``TestScope`` prends en paramètre de constructeur un dictionnaire de concordance entre un type et une instance, vous permettant de retourner l'implémentation désirée pour le test selon un type donné. Le ``TestScopeFactory`` permet d'avoir cet enregistrement à un niveau plus général et injectera ce dictionnaire de concordance à chaque scope de test créé ::

    var testScopeFactory = new TestScopeFactory();
    testScopeFactory.Instances.Add(typeof(IMyAbstraction), myImplementationVar);
    
    var scope = testScopeFactore.CreateScope(); // Scope will contain myImplementationVar "registration"
    
    var instance = scope.Resolve(); // Instance will be same object as myImplementationVar
    var instance2 = new TestScope(new Dictionary{ {typeof(IMyAbstraction), myImplementationVar} }); // will be the same result as previous line

Une instance du ``TestScopeFactory`` est mis à disposition dans le BaseUnitTestClass.

Bus
^^^

Il peut arriver que vous ayez besoin, dans le cadre bien définit d'une méthode donnée, directement d'un bus. Pour répondre à ce problème, nous avons fourni deux implémentation test, ``FakeCommandBus`` et ``FakeEventBus``. Ces deux bus implémente respectivement ``ICommandBus`` et ``IEventBus``, et fournissent sous forme d'un ``IEnumerable`` public la liste des commandes/events qui ont publiées par leur biais.

.. note:: Si vous avez des besoins de tests plus avancés, nous vous recommandons d'utiliser les bus in-memory, plus complexes et plus lents, mais plus extensibles et configurables. L'utilisation des FakexxxBus est recommandé uniquement pour des tests unitaires extrêmement simples où le bus est directement passé en tant que dépendance à la classe.

Test du dispatch
^^^^^^^^^^^^^^^^

Bien que nous ayons vu ci-dessus l'existence de deux bus de tests pour simuler les envois d'informations dans le système, il peut-être utile de se placer un cran dessus et vérifier le comportement du dispatcher. Il y a deux modes de fonctionnement pour le dispatcher : l'utilisation d'une instance qui implémente ``IDispatcher`` et l'utilisation de la version statique ``CoreDispatcher``.

Ce qui va intéresser le développeur est de savoir si sa commande/son évenement a bien été publié, si plusieurs commandes/évenements ont été publiés ou au contraire, si aucun ne l'ont été, afin de s'assurer du comportement attendus. Nous avons créé une classe statique, ``Test``, qui permet de s'assurer de cela.

La classe test s'applique sur un contexte d'exécution donné (méthodes ``When`` et ``WhenAsync``). Il est possible de passer un mock d'une instance de ``IDispatcher`` afin d'effectuer les vérifications sur ce dernier plutôt que sur le ``CoreDispatcher`` statique. Lorsque le contexte est créé, on récupère la possibilité d'effectuer un test sur l'exécution du contexte. Toutes les méthodes disposent de la possibilité de passer un timeout en millisecondes afin d'éviter d'avoir des tests trop longs (fixé par défaut à 1 sec). La liste des méthodes de test possibles sont :

- ``ThenNoEventShouldBeRaised`` : vérifie qu'aucun évènement n'est levé à la suite de l'appel du contexte
- ``ThenNoCommandAreDispatched`` : vérifie qu'aucune commande n'est envoyée à la suite de l'appel du contexte
- ``ThenEventShouldBeRaised<T>`` : vérifie qu'un évènement, du type donné, est levé à la suite de l'appel du contexte. Si plusieurs évènements sont publiés, uniquement le dernier évènement de type T sera renvoyé
- ``ThenCommandIsDispatched<T>`` : vérifie qu'une commande, du type donné, est publiée à la suite de l'appel du context. Si plusieurs commandes sont publiées, uniquement la dernière de type T sera renvoyée
- ``ThenEventsShouldBeRaised`` : vérifie que que plusieurs évènements sont publiés à la suite de l'appel du contexte
- ``ThenCommandsAreDispatched`` : vérifie que plusieurs commandes sont publiées à la suite de l'appel du contexte
- ``ThenNoMessageShouldBeRaised`` : vérifie qu'aucun message n'a été envoyé à la suite de l'appel du contexte. Attention, cette méthode n'est évaluée que sur le CoreDispatcher
- ``ThenMessagesShouldBeRaised`` : vérifie que plusieurs messages ont été envoyés à la suite de l'appel du contexte. Attention, cette méthode n'est évaluée que sur le CoreDispatcher
- ``ThenMessageShouldBeRaised<T>`` : vérifie qu'un mesage, du type donné, est envoyée à la suite de l'appel du contexte. Attention, cette méthode n'est évaluée que sur le CoreDispatcher

::

    var evt = await Test.WhenAsync(myAsyncMethod).ThenEventShouldBeRaised();
    //Perform tests on evt instance ...

Méthode d'extensions
^^^^^^^^^^^^^^^^^^^^

Pour conclure, nous avons ajouté un ensemble de méthodes d'extensions. Ces méthodes concernent aussi bien nos plugins que nos assemblies de base que les packages communautaires que nous incluons par défaut.

Au niveau DDD, la méthode ``ClearDomainEvents`` permet, sur une instance d'un ``AggregateRoot``, de nettoyer la collection d'évènements ajoutés par le biais des méthodes AddDomainEvent de l'aggregat, vous permettant de vider la collection d'évènements entre plusieurs appels pour faire vos assertions.

Au niveau DAL, nous avons donné la possibilité de

- ``FakePersistenceId`` : permet d'effectuer le set de la propriété Id d'un PersistableEntity
- ``SetupSimpleGetReturns`` : permet de définir extrêmement facilement sur un mock d'un ``IDataReaderRepository`` ce que la méthode ``GetAsync`` doit renvoyer (en fournissant une liste d'élément finie in-memory)
- ``VerifyGetAsyncCalled`` : à l'instar de la méthode précédente, permet de vérifier extrêmement facilement si la méthode ``GetAsync`` a été appelée sur un mock d'un ``IDataReaderRepository``

Au niveau MVVM (package ``CQELight.TestFramework.MVVM``), nous avons définit une méthode, ``GetStandardViewMock`` qui permet de retourner un mock par défaut de l'interface ``IView``. La spécificité de ce mock est qu'il contient déjà la méthode de callback pour la méthode ``PerformOnUIThread``, de façon à ce que cette dernière s'exécute de façon systématique en contexte de test unitaire.