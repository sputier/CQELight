DAL avec EF Core
================

Entity Framework Core permet d'accéder aux données configurée à l'aide de la couche DAL de CQELight. Pour pouvoir l'utiliser, il est nécessaire de créer un contexte propre à votre couche de données qui hérite de ``CQELight.DAL.EFCore.BaseDbContext`` :

::

    public class MyDbContext : CQELight.DAL.EFCore.BaseDbContext 
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {        
        }
    }

.. warning:: Pensez à créer cette classe dans le même projet que vos modèles qui sont mappés à l'aide des attributs de CQELight, car une étape de configuration automatique est faite dans la classe BaseDbContext qui recherche automatiquement tous les modèles de son projet pour les mapper dans le moteur Entity Framework Core.

A partir de cette étape, vous avez accès à tous vos objets à l'aide du context préalablement défini. Vous pouvez donc utiliser toutes les fonctionnalités d'Entity Framework Core. Cependant, en faisant cela, vous n'utilisez pas les objets repository (``IDataReaderRepository``, ``IDataUpdateRepository`` et ``IDatabaseRepository``), ce qui vous oblige à utiliser directement les API EF Core partout dans le code.

Bien que cela soit fonctionnel, ce n'est pas optimal, car cela lie très fortement votre code d'accès aux données à Entity Framework Core, vous empéchant ainsi d'utiliser autre chose (MongoDb, NHibernate, ...). Il est donc conseillé d'utiliser directement les objets repository dans votre code métier.

.. warning:: Ceci nécessite de mettre en place un plugin IoC pour profiter de l'injection de dépendances automatiquement.

L'utilisation de la méthode d'extension du Bootstrapper vous permettra de réaliser cette opération d'enregistrement sans aucun effort sous deux formes :

- Soit en référençant une instance de contexte unique à l'ensemble de votre application (attention, beaucoup de risques de problèmes avec des accès concurrents)
- Soit en référençant des options de contexte pour votre application (option recommandé, le système gère et crée un contexte quand il en a besoin)

De plus, certaines options supplémentaires peuvent être fournies pour déterminer le comportement global de l'accès aux données par le moteur EF Core :

- ``DisableLogicalDeletion`` : désactive de façon globale la suppression logique (pour n'utiliser que la suppression physique). Cela évite de devoir préciser le flag à chaque suppression.

Ces options sont rajoutées après la configuration des contextes.

::

    //With global unique context
    new Bootstrapper()
        .UseEFCoreAsMainRepository(new MyDbContext(myDbOptions));
    
    //With global options for all context in all assemblies
    new Bootstrapper()
        .UseEFCoreAsMainRepository(myDbOptions);
    
    //With options
    new Bootstrapper()
        .UseEFCoreAsMainRepository(myDbOptions, new EFCoreOptions { DisableLogicalDeletion = true });

A la suite de cette opération, chaque repository qui sera injecté dans votre code utilisera la couche d'accès aux données EF Core pour effectuer ses opérations et verra injecter un ``EFRepository<T>`` (ou votre sous-instance définie et enregistrée dans le container par vos soins).

.. note:: Comme précisé dans la page sur l'accès aux données, il est toujours plus intéressant d'utiliser vos héritages de repository afin d'avoir la pleine main sur ce que vous voulez faire. Ici, il s'agirait d'avoir un héritage de ``EFRepository`` par modèle afin d'avoir des fonctions plus fines. Vous serez de fait assuré d'avoir le contexte qui vous est nécessaire injecté en paramètre de votre repository, si vous enregistrez ce dernier également dans le container IoC.