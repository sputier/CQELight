Bootstrapper
============
Généralités
^^^^^^^^^^^
Le bootstrapper est le point d'entrée principal du système, qui permet de le configurer comme voulu afin de s'adapter à votre application, votre infrastructure et votre métier. Il permet de mettre en relation les différentes extensions de CQELight avec le coeur du système. Il existe plusieurs types d'extensions :

- **Container IoC** : il s'agit des extensions qui résident au coeur de tout le système, car c'est dans cette extension que sont enregistrés les liens entre les abstractions et les implémentations.
- **Bus** : il s'agit des extensions qui définissent un bus de messaging, utilisé pour transporter les évènements et les commandes dans le système.
- **DAL** : il s'agit des extensions qui permettent d'abstraire l'accès aux donnés, utilisés majoritairement par la couche Query, et de fournir des implémentations pour la couche repository.
- **EventStore** : il s'agit des extensions qui managent le système évènementiel, se chargeant de la persistance et de la récupération des évènements qui sont arrivés dans le système.
- **Autre** : il s'agit de toutes les extensions voulant profiter de ce qu'on CQELight en terme de flexibilité et d'outils pour permettre une intégration facilitées. On y trouvera également des extensions commerciales qui ne répondent pas à un type ci-dessus.

Toutes ces extensions doivent être configurées et injectées dans le boostrapper, par le biais d'une classe implémentant l'interface IBoostrapperService. Cette implémentation doit définir le type d'extension dont il s'agit ainsi qu'une méthode callback qui effectue de façon lazy le boostrapping (ceci étant dû à des mécanismes internes d'initialisation). Cette méthode d'initialisation prends mets à disposition un paramaètre, de type BootstrappingContext que les extensions peuvent exploiter pour avoir plus d'informations au moment de leur bootstrapping propre.

.. note:: A noter qu'aucune extension n'est obligatoire. Cependant, le système sera limité voire inopérant s'il manque des services. Le cas de l'IoC doit être traité avec une extrêmement vigilance car ce n'est pas toujours disponible. L'information est à votre disposition lors de la méthode de bootstrapping, dans le BootstrappingContext.

Voici une définition de bootstrapper "classique" ::
  
    new Bootstrapper()
       .ConfigureCoreDispatcher(GetCoreDispatcherConfiguration())
       .UseInMemoryEventBus(GetInMemoryEventBusConfiguration())
       .UseInMemoryCommandBus()
       .UseEFCoreAsMainRepository(new AppDbContext())
       .UseSQLServerWithEFCoreAsEventStore(Consts.CONST_EVENT_DB_CONNECTION_STRING)
       .UseAutofacAsIoC(c => { })
       .Bootstrapp();
        
La méthode Bootstrapp retourne une liste de notifications. Cette liste contient un ensemble de notifications émises soit par le système soit par les extensions. Les notifications sont de trois niveaux : ``Info``, ``Warning`` et ``Error``.

.. note:: Il reste dans la responsabilité du développeur de consulter et d'exploiter cette liste, aucune exception n'était renvoyée lors du process de bootstrapping (sauf si configuré, voir ci-dessous). Il est recommandé d'arrêter le processus de démarrage d'une application si une notification de type 'Error' survient.

Il est possible de configurer plus finement le comportement du bootstrapper en lui passant en paramètre de constructeur un instance de ``BootstrapperOptions``. Cette classe contient plusieurs flags :

- ``Strict`` : passer cette valeur à true impliquera que, hors extensions de type Bus et Autre, il est impossible d'enregistrer plus d'une extension pour un type de service donné (par exemple, impossible d'avoir deux extensions de type IoC).
- ``CheckOptimal`` : passer cette valeur à true impliquera que, lors de la méthode Bootstrapp, il y aura vérification qu'au moins un service de chaque type sera enregistré (exception faite du type "Autre"). Le système fonctionnera donc de la meilleure façon possible.
- ``ThrowExceptionOnErrorNotif`` : passer cette valeur à true provoquera une exception si une ou plusieurs notifications de sont créées durant le processus de bootstrapping et que ces dernières sont de niveau 'Error'. L'exception sera de type ``BootstrappingException``.
- ``AutoLoad`` : passer cette valeur à true indiquera au bootstrapper qu'il doit tenter de charger de façon automatique les plugins exportant un comportement de chargement automatisé. Cela permet d'éviter la phase de configuration manuelle à l'aide d'appel de méthode d'extension, mais en contre-partie, la configuration par défaut est appliquée.


Post-bootstrapping
^^^^^^^^^^^^^^^^^^

Il peut être intéressant de se brancher en fin de process de bootstrapping pour effectuer des actions particulières. Il existe un event, ``OnPostBootstrapping`` auquel il est possible de s'abonner, qui sera invoqué en fin de process (peu importe l'issue). Cet event met à disposition un contexte, la classe ``PostBootstrappingContext``. Cette dernière contient la liste des notifications du processus ainsi qu'un scope de résolution du container IoC (si un plugin a été configuré, sinon la valeur null est affectée).
