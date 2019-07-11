Configuration du dispatcher
===========================
Généralités
^^^^^^^^^^^

Le dispatcher est le chef d'orchestre du système, permettant de délivrer les informations en faisant le lien entre la demande d'envoi et le bus de destination, voire parfois même le destinataire, tout en fournissant une API simplifiée et accessible. Il existe deux versions du dispatcher: une API statique, le ``CoreDispatcher``, et une API d'instance, implémentation de l'interface ``IDispatcher``. 

CQELight fourni une implémentation de cette interface (utilisée par le ``CoreDispatcher``) : le ``BaseDispatcher``. Il est recommandé, de façon générale, d'utiliser la version d'instance plutôt que la version statique, pour des contraintes d'accès concurrentiels et de performances. Cela rends également le code plus explicite en marquant le dispatcher comme étant une dépendance nécessaire au fonctionnement d'une classe donnée.

Bien qu'il soit recommandé d'utilisé la version d'instance, dans certains projets (comme les applications desktop), il y a quelques avantages d'utiliser en plus la version statique. En effet, le ``CoreDispatcher`` permet également de stocker des références vers certains parties du système, dans le processus en cours. Ainsi, on pourra lui demander d'avoir une référence vers un objet donné, pour capter des évènements, dans le contexte courant, grâce à la méthode ``AddHandlerToDispatcher``, qui prends une instance dérivant de la classe object en paramètre (donc fondamentalement n'importe quel type système). Cet objet doit être un ``IDomainEventHandler``, ``ICommandHandler`` ou ``IMessageHandler`` afin d'être ajouté et éligible lors de l'envoi.

Le ``CoreDispatcher`` est également le seul à pouvoir transmettre des messages applicatifs, qui implémentent l'interface ``IMessage``. Ces messages sont souvent utilisés dans des contextes MVVM (WPF/Xamarin), exclusivement in-process, afin de découpler les interactions en lien entre View et ViewModel.

Etant donné le rôle central qu'a le dispatcher, il faut qu'il puisse être configuré finement afin d'être sûr que chaque envoi d'informations dans le système arrivent bien à destination. Il est possible de fournir une configuration, à l'aide du fluent builder de configuration, le ``DispatcherConfigurationBuilder``.

.. note:: En l'absence de configuration, le dispatcher utilise la configuration par défaut, qui consiste à envoyer chaque évènement/commande à chaque bus qui a été défini dans le bootstrapper, sans aucune gestion d'erreur, sérialisés en JSON.

En sélectionnant un type spécifique, ou un ensemble de type (par le biais du namespace par exemple), on peut appliquer des choix comportementaux. Les élements configurables sont :

- L'envoi sur un ou plusieurs bus. Cela permet de définir par exemple, quels évènements sont des évènements internes au contexte, et lesquels doivent être publiés extérieurement.
- L'utilisation d'un moteur de sérialisation. Cela est nécessaire si le transport est particulier, auquel cas, le bus récupèrera l'instance du moteur de sérialisation et pourra l'utiliser si nécessaire.
- Un callback de gestion des erreurs s'il y a une ou plusieurs exceptions. Ce callback récupère l'exception rencontrée et permet de définir un traitement.
- La définition si le ou les type(s) choisi(s) est/sont "SecurityCritical", qui permet de définir si c'est un clone de l'instance qui est envoyé aux custom callback, ou si c'est l'instance réelle (ouvrant une porte à une modification des propriétés par un custom callback).

Il faut donc appeler le ``ConfigurationBuilder`` afin de pouvoir définir le comportement à adopter ::

    var builder = new DispatcherConfigurationBuilder();
    
    builder
        .ForAllEvents()
        .UseAllAvailableBuses()
        .HandleErrorWith(e => { Console.WriteLine(e); })
        .IsSecurityCritical()
        .SerializeWith<JsonDispatcherSerializer>();
    
    builder
        .ForAllCommands()
        .UseAllAvailableBuses()
        .HandleErrorWith(e => { Console.WriteLine(e); })
        .IsSecurityCritical()
        .SerializeWith<JsonDispatcherSerializer>();
    
	//Get the configuration
    var config = builder.Build();
	
	//Apply it to current system
	new Bootstrapper()
		.ConfigureDispatcher(config)
		.Bootstrapp();
    
On récupère la configuration en appelant la méthode ``Build()``. Il est possible de spécifier un paramètre 'strict' au build de la configuration. La définition de sa valeur à 'true' vérifie que tous les events et les commands sont assignés à un bus minimum. Ca permet d'assurer qu'il n'y a pas de type qui sont orphelins et ne seront pas traités lors d'un dispatch. A noter que cette valeur est mise à false par défaut.

Une fois la configuration récupérée, on la passe en paramètre au bootstrapper pour l'appliquer au système (voir l'article sur le :doc:`bootstrapper` pour plus de détails).