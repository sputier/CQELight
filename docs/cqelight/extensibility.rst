Créer vos propres extensions
============================
Généralités
-----------

CQELight a été initialement conçu afin d'être hautement extensible, en fonction des besoins de chaque projet et de chaque équipe. A cet effet, il est possible, tout comme les extensions officielles, de créer vos propres extensions. Le processus se veut être assez simple.

Pour développer une extension, il est nécessaire de savoir de quel type d'extension il s'agit. Il y a cinq types d'extensions possibles :

- Gestionnaire d'IoC
- Service de bus messaging
- Service d'accès aux données
- Event store
- Autre

Une fois le type d'extension défini, il faut passer par plusieurs étapes intermédiaires afin d'en créer une.
Afin de conserver la logique modulaire, il est fortement conseillé de faire un package par extension, au cas où les besoins de votre projet viendrait à évoluer. Une extension est un nouveau projet de type 'Bibliothèque de classes' (de préférence en .NET Standard 2.0). Une fois le nouveau projet créé dans Visual Studio, il faut y définir les élements nécessaire pour la configuration:

- Une classe service qui sera ajoutée à la collection du bootstrapper
- La méthode d'extension du Boostrapper qui vous permettra de configurer votre extension de façon fluide et moderne (du style UseXXX)

Création de l'extension
-----------------------

Création de la classe de service
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Comme expliqué dans la doc sur le bootstrapper, chaque extension possède une classe de service qui implémente ``IBoostrapperService``, dans le but d'apporter une certaine cohésion dans la gestion de votre extension. Cette classe doit implémenter l'interface ``CQELight.IBootstrapperService`` ::
   
    internal class MyAwesomeBusExtensionService : CQELight.IBootstrapperService
    {
        public BootstrapperServiceType ServiceType => BootstrapperServiceType.Bus;
        
        public Action BootstrappAction { get; internal set; } = (ctx) => { };
    }
  
L'interface impose la défintion de deux membres :

- ``ServiceType``, correspondant à l'énumération pour préciser de quel type de service il s'agit.
- ``BootstrappAction``, étant l'action éxécutée lors du bootstrapping. Cette méthode possède à sa disposition le ``BootstrappingContext`` permettant d'avoir plus d'infos sur l'état du système lors de votre bootstrapp.

Lorsque cette classe de service est faite, il est nécessaire d'ajouter une instance de cette dernière dans la collection des services du bootstrapper (dans la méthode d'extension de configuration).

.. note:: Il est obligatoire de passer par une méthode de bootstrapping qui sera exécutée plus tard dans le process, afin de permettre au système de faire des évaluations et traitements avant que chaque extension soit réellement initialisée. Si vous prenez le parti de faire directement des instanciations lors de votre méthode d'extension, vous vous exposez à des effets de bord indésirables.

Méthode d'extension du bootstrapper
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Toutes les extensions étant bootstrappées au lancement de l'application selon un ordre défini par le framework, il faut fournir un point d'entrée pour indiquer que l'on veut utiliser la vôtre. Il est recommandé de procéder au bootstrapping au lancement de votre application afin de permettre que la configuration soit centralisée, et d'avoir le système prêt le plus tôt possible (dans le ``Startup`` d'une application AspNet ou dans le ``App.xaml`` d'une application WPF par exemple). Afin de configuer le bootstrapper, il faut appeler les méthodes nécessaires sur une instance de la classe ``Bootstrapper``, qui sont généralement des méthodes d'extensions. Il vous faut alors faire une méthode d'extension sur la classe ``Bootstrapper`` pour permettre d'appeler l'initialisation de votre extension.

Cette méthode d'extension s'applique sur la classe ``CQELight.Bootstrapper``, et doit retourner l'instance initiale, afin de permettre d'enchainer les appels de configuration. Le but ici est fourni la méthode de callback qui sera appelée par le système dans l'ordre défini (qui n'est pas l'ordre d'appel des méthodes d'extension) pour préparer le contexte général propice à votre extension (injection de type dans le container IoC, définition de variable statiques, etc...). Vous devrez alors utiliser ajouter votre classe de service au bootstrapper après l'avoir implémentée.

.. note:: Attention : l'utilisation d'un container IoC n'est pas obligatoire pour utiliser CQELight, il s'agit d'une extension au même titre qu'une autre. De ce fait, il est fortement recommandé que, même si vous utilisez l'injection de dépendances dans votre extension, vous n'en fassiez pas quelque chose obligatoire (sauf si vous avez la maitrise totale sur le système globale), de peine de se priver d'un public potentiel pour votre extension. Vous pouvez consulter le BoostrappingContext pour savoir si une extension IoC est définie.

::

    public static Bootstrapper UseMyAwesomeExtension(this Bootstrapper bootstrapper, ...custom params...)
    {
        var service = new MyAwesomeExtensionService();
        service.BootstrappAction += (ctx) =>
           {
               bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(MyImplementation), typeof(IMyAbstraction), typeof(MyImplementation)));
           };
    
        if (!bootstrapper.RegisteredServices.Any(s => s == service))
        {
            bootstrapper.AddService(service);
        }
        return bootstrapper;
    }
       
Par convention, cette classe se trouve à la racine de votre projet et se nomme ``Bootstrapper.ext``. Il faut cependant préciser que cette classe ne contient que vos méthodes d'extensions et que le nommage ne change rien au fonctionnement général.

.. note:: Il est recommandé de faire une méthode d'extension sur le bootstrapper et de retourner l'instance en paramètre pour permettre une fluent configuration. Cependant, rien ne l'oblige dans votre propre projet. C'est par contre un élement obligatoire si vous souhaitez que votre extension rejoigne la liste officielle des extensions CQELight.

Définition du contenu de l'extension
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Ici, il s'agit de votre extension, c'est à vous d'en définir son implémentation.

Par contre, vous pouvez voir plusieurs exemples sur comment réaliser une extension  `sur notre GitHub <https://github.com/cdie/CQELight/tree/master/src>`_ (chaque package est une extension).

Après demande de votre part (remplir une issue sur notre GitHub), vous pouvez demander à ce que votre extension rejoigne la collection officielle des extensions CQELight, publiée sur NuGet, avec la documentation hébergée par Hybrid Technologies Solutions. Ceci passe par une étape de review de code et de test, ainsi que de la mise en confirmité avec nos standards. Vous pouvez également participer à l'élaboration des extensions officielles existantes qui sont open-source.

Lors de la création de votre méthode bootstrapping, vous aurez accès à un contexte de bootstrapping. Ce contexte contient un ensemble d'information vous permettant de configurer plus finement votre extension. Vous y trouverez entre autre :

- Les flags passés au constructeur du bootstrapper, strict et optimal (voir documentation du bootstrapper pour comprendre la signification). Ces flags vous permettent de configurer votre extension en fonction des contraintes voulues par l'appelant général.
- Une méthode ``IsServiceRegistered`` qui permet de savoir si un service d'un type donné a déjà été défini (comme par exemple un service de type IoC pour effectuer des injections IoC).
- Une méthode ``IsAbstractionRegisteredInIoC`` qui permet de savoir si un type abstrait a déjà été défini dans le container du bootstrapper. Attention cependant, cette méthode ne garantit en rien qu'un telle association n'ait pas été faite en dehors du bootstrapper. Le cas échéant, l'information n'est pas disponible par le biais de cette extension.

Selon les flags qui vous sont passés et les besoins de votre extension, il est possible d'ajouter des notifications au niveau du bootstrapper. La classe ``CQELight.Bootstrapper`` expose deux méthodes, ``AddNotification`` et ``AddNotifications`` qui vous permettent de réaliser cette opération. Vous pouvez créer une notification en précisant le type de notification (Info, Warning, Error) ainsi qu'un message, et il également possible de fourni le type de service qui a créé cette notification.

Spécificités de chaque type
---------------------------
Extension IoC
^^^^^^^^^^^^^
Si vous développez une extension pour la gestion d'un container IoC, il est impératif de gérer les types qui ont été enregistrés dans le bootstrapper par les autres extensions. Voir la documentation sur l':doc:`ioc` pour savoir les différents types d'enregistrements à gérer.

Une extension de type IoC doit également prendre en charge les interfaces ``IAutoRegisterType`` et ``IAutoRegisterTypeSingleInstance``, qui sont des raccourcis pour permettre l'enregistrement de type dans le container IoC sans en maitriser la particularité.