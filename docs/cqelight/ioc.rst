Inversion of Control
====================
Généralités
^^^^^^^^^^^

`L'IoC (inversion of control) <https://en.wikipedia.org/wiki/Inversion_of_control>`_ est une pratique de développement logiciel consistant à coder uniquement avec des abstractions et utiliser un container afin de récupérer la bonne implémentation selon le contexte. Ceci permet au code métier d'être hautement extensible (on change une implémentation sans changer la logique), testable (on peut définir nos implémentations de test pour piloter un comportement) et plus facilement maintenable (les abstractions sont clairement séparées des implémentations).

Ce concept est souvent couplé à l'injection de dépendance (Dependency Injection) qui consiste à stipuler que les dépendances abstraites doivent être fournies à la construction par un mécanisme d'injection.

De ce fait, CQELight permet l'utilisation de l'IoC et l'injection de dépendance dans son système, bien que cela ne soit pas obligatoire pour l'ensemble du système. Les avantages cités ci-dessus s'appliquent également dans le cas présent, et certaines extensions font un usage intensif de l'IoC, comme beaucoup de systèmes modernes (par exemple la configuration Asp.net Core).

Enregistrement
^^^^^^^^^^^^^^

L'enregistrement est, de façon générale, géré lors de l'appel à la méthode ``Bootstrapp()`` de l'extension IoC. Il faudra alors que l'extension IoC se charge de récupérer les enregistrements du système pour les traiter selon ses spécificités. Le cas particulier est l'enregistrement de résolution lors de la création d'une extension, si vous voulez profiter de la puissance offerte d'un container Ioc, de vouloir faire un enregistrement. Dans ce cas précis, vous ne pourrez utiliser que les types d'enregistrements offerts par le système et non les spécificités du container utilisé.

Pour ce cas particulier, il faut passer par une collection interne au bootstrapper. Ce dernier fourni un point d'entrée simplifié : ``AddIoCRegistration``. Cet appel doit être fait dans la méthode d'extension du bootstrapper (pour en savoir plus, lisez la documentation sur l':doc:`extensibility`).

Il y a trois façon d'enregistrer dans le bootstrapper : par type, par instance et par factory. La différence réside dans le mode de résolution. Un enregistrement par type donnera à chaque résolution une nouvelle instance, un enregistrement par instance donnera l'instance qui a été enregistrée (singleton) et un enregistrement par factory permettra d'exécuter une logique de création/récupération personnalisée (invoquée à chaque résolution). Si cela s'avère insuffisant, il est toujours possible d'utiliser les méthodes natives du container par le biais de la méthode de bootstrapping.

.. note:: Les plugins officiels CQELight d'IoC permettent de configurer le container à l'aide des outils offerts par ce dernier en plus des types de CQELight. Il est fortement conseillé aux créateurs de plugins IoC d'en faire de même 

::

    // Register by type - Need implementation type and corresponding abstract types
    bootstrapper.AddIoCRegistration(new TypeRegistration(typeof(InMemoryCommandBus), typeof(ICommandBus), typeof(InMemoryCommandBus)));
                
    // Register by instance - Need the instance and corresponding abstract types
    bootstrapper.AddIoCRegistration(new InstanceTypeRegistration(configuration, typeof(InMemoryEventBusConfiguration)));
    
    // Register by factory - Need a lambda and corresponding abstract types
    bootstrapper.AddIoCRegistration(new FactoryRegistration(() => efRepoType.CreateInstance(dbContext),
                                    dataUpdateRepoType, databaseRepoType, dataReaderRepoType));

.. note:: Attention cependant, si aucune extension d'IoC n'a été configurée, vos enregistrements seront faits en vain. Bien que l'IoC soit fortement recommandé, il n'est pas obligatoire, il est préférable de toujours garder une possibilité hors IoC, même si cette dernière est fortement limitée.

Résolution
^^^^^^^^^^
De base, l'injection de dépendances est faite par le biais des constructeurs. Vous pouvez, dès lors que vous avez activé l'utilisation d'une extension IoC, passer vos abstractions dans les constructeurs (de vos handlers d'events ou commands par exemple), qui seront automatiquement résolues par le système sans que vous vous en préoccupiez.

Il s'agit de la méthode de récupération des objets depuis le container la plus recommandée. Cependant, il est possible de faire des résolutions manuelles. A cet effet, il est prévu une notion de scope. Un objet résolu n'est garanti valide que dans le cadre d'un scope donné. Si scope est terminé, il est possible que l'objet résolu ne soit plus dans un état consistant.

CQELight fourni une API pour la résolution à n'importe quel moment de votre code, autre que le constructeur. Il y a deux façons de récupérer un scope de résolution : l'API statique (``DIManager``) ou l'utilisation d'un ``IScopeFactory``. Un ``IScopeFactory`` étant un type abstrait, il est nécessaire de l'avoir en dépendance dans le constructeur :: 

    using(var scope = DIManager.BeginScope())
    {
        var implementation = scope.Resolve<IAbstraction>();
    }
    
    public MyClass(IScopeFactory scopeFactory) // Ctor
    {
        using(var scope = scopeFactory.BeginScope())
        {
            var implementation = scope.Resolve<IAbstraction>();
        }
    }
	
.. note:: Attention à la durée de vie. La majorité des containers IoC en .NET gèrent eux-mêmes la durée de vie des objets qu'ils ont résolus. De fait, dans l'exemple ci-dessus, si ``IAbstraction`` est un ``IDisposable``, l'appel de la méthode Dispose sera faite en même temps que celle du scope.

Comme souvent, la méthode d'instance est fortement recommandée si vous en avez la possibilité. Il peut arriver que parfois il soit nécessaire de passer par l'API statique (méthode statique, pas de possibilité de modifier le constructeur, impossible de se faire injecter un type dans le constructeur, ...).

L'utilisation de l'API du ``DIManager`` est conditionnée à l'appel de la méthode ``DIManager.Init()`` qui prends en paramètre un ``IScopeFactory``. Généralement, cet appel est réalisé par les plugins d'IoC de CQELight. Si vous développez un plugin pour un container IoC, pensez à faire cet appel au bootstrapp de votre extension.

Spécificités
^^^^^^^^^^^^
Paramètres de résolutions
^^^^^^^^^^^^^^^^^^^^^^^^^

Généralement, une résolution est faite sans nécessité de préciser des paramètres particuliers. Il arrive cependant que certains types aient besoin d'un ou plusieurs paramètres pour que la résolution se fasse (si ces paramètres sont dynamiques à l'exécution). Pour les paramètres que le container IoC connait, la majorité de ces derniers arrivent à les gérer sans aide. Par contre, il peut arriver qu'il y ait besoin de paramètres spécifiques non résolvables.

Pour gérer ces derniers, il y a deux façons de préciser un paramètre lors de sa résolution : par nom ou par type. S'il n'y a qu'un paramètre spécifique, ou plusieurs dont le type est différent, la résolution par type est possible (et recommandée). Si ce n'est pas possible (par exemple deux paramètres de type string), alors la résolution par nom entre en jeu.

Pour résoudre un objet en précisant un paramètre par son type, il faut faire l'appel de la façon suivante :
:: 

    using(var scope = _scopeFactory.GetScope())
    {
        var instance = scope.Resolve(new TypeResolverParameter(typeof(string), "value"));
    }
	
Pour résoudre un objet en précisant un paramètre par son nom, il faut faire l'appel de la façon suivante :
::

    using(var scope = _scopeFactory.GetScope())
    {
        var instance = scope.Resolve<IAbstraction>(new NameResolverParameter("param1", "value"));
    }
 
.. note:: Attention, certains providers IoC ne supporte pas nativement ce comportement particulier (comme par exemple ``Microsoft.Extensions.DependencyInjection``. Vérifiez que votre provider le supporte ou vous risquez d'avoir une exception à l'exécution.