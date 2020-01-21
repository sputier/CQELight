DAL avec Mongo
==============

MongoDb est un fournisseur de base de données sous format documentaire largement utilisé et répandu. Il est livré avec son driver C#, permettant de le piloter de bout en bout.
Du fait de sa grande flexibilité, il n'est pas nécessaire d'utiliser les classes de bases fournies par le framework, toute classe pouvant être utilisée. 

Afin d'activer la prise en charger de MongoDb, il faut ajouter le package correspondant à l'extension : ``CQELight.DAL.MongoDb``. A la suite de l'installation, il suffit de configurer le Bootstrapper avec les options de votre base de données. Les options se configurent dans la classe ``CQELight.DAL.MongoDb.MongoDbOptions``. 
::

    new Bootstrapper()
        .UseMongoDbAsMainRepository(new MongoDbOptions(mongoUrl));
    
.. note:: Il est recommandé de configurer les options MongoDbOptions avec une instance de MongoUrl qui laisse plus de flexibilité pour les déploiements d'infrastructure.

A partir de cette étape, votre connexion à MongoDb est correctement configurée. 
Si vous utilisez un système d'injection de dépendances, les types repository seront disponibles pour l'injection (``IDataReaderRepository``, ``IDataUpdateRepository`` et ``IDatabaseRepository``). Si vous avez une besoin avancé nécessitant un accès directement au client MongoDb, il est possible d'y accéder en se le faisant injecter (il s'agit d'un singleton) ou directement grâce à ``CQELight.DAL.MongoDb.MongoDbContext.Client``

.. note:: Comme précisé dans la page sur l'accès aux données, il est toujours plus intéressant d'utiliser vos héritages de repository afin d'avoir la pleine main sur ce que vous voulez faire. Ici, il s'agirait d'avoir un héritage de ``MongoRepository`` par modèle afin d'avoir des fonctions plus fines. Vous serez de fait assuré d'avoir le contexte qui vous est nécessaire injecté en paramètre de votre repository, si vous enregistrez ce dernier également dans le container IoC.