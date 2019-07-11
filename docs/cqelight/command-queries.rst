Commands et Queries
===================
Séparation CQRS
^^^^^^^^^^^^^^^

Nous avons vu comment modéliser le domaine dans l'article :doc:`domain-modeling`. Lorsque que nous devons penser, au niveau domaine, à l'API publique, on pense généralement aux méthodes (et propriétés accessibles éventuelles) de l'aggregat.

En respectant le pattern CQRS, il faut faire une distinction entre les interactions avec le système provoquant une modification (commands) et les lectures de données (queries). Le pattern stipule que l'exécution d'une query doit se comporter comme une fonction mathématique pure, à savoir qu'un appel identique doit fournir un résultat identique s'il n'y a pas eu d'altération du système entre temps.

De l'autre côté, une commande va modifier le système, à savoir que chaque appel va mettre le système dans un état différent. On considère généralement qu'une query a un type de retour selon ce qu'on veut requêter et une commande retourne ``void`` (ou ``Task`` pour les commandes asynchrones).

Si l'on utilise le système événementiel (soit en event sourcing, soit en event driving), la réaction de notre aggregat à une commande sera de créer un ou plusieurs événements et de les ajouter dans sa collection interne. L'appelant sera alors chargé de regarder dans cette collection pour voir le résultat de son appel.

Si on n'utilise pas le système d'évènement métier, on va alors utiliser une notion de résultat d'appel. Ce retour doit contenir la valeur métier de l'échec ou de la réussite. CQELight expose une classe permettant d'encapsuler ce retour, la classe ``Result``. Cette classe de résultat peut être utilisée au niveau de l'API publique que donne l'aggregat (sans utilisation des évènements) ou dans les objects dépendants de l'aggregat (entités et valueObjects). Cela permet d'encapsuler la valeur de retour et de garder la signature de notre méthode honnête (en stipulant que l'action peut avoir un résultat qui peut être positif ou négatif selon les cas).

.. note:: De façon générale, lorsqu'on implémente le code du domaine, il est préférable d'éviter d'utiliser les exceptions car le système de gestion des exceptions est beaucoup plus lourd et coûteux que celui des valeurs de retours. De même, dans une logique de programmation, on doit garder les exceptions pour des cas exceptionnels et non pour des résultats métiers attendus.

La classe ``Result`` se présente sous deux formes : une avec valeur et une sans valeur. On ne distingue le fait, que ça soit un échec ou succès, que par le flag ``IsSucces``. Cette classe permet d'éviter l'utilisation d'un type primitif tel que le booléen qui ne transporte pas assez d'informations métier.

::     

    public enum TransferCannotBeDoneBecause
	{
	    NotEnoughMoney
	}

    public Result TransferMoney(Account from, Accout to, Amount amount)
    {
        // Business logic
        if(!from.HasEnoughMoney(amount))
        {
            return Result.Fail(TransferCannotBeDoneBecause.NotEnoughMoney);
        }
        return Result.Ok();
    }
	
.. note:: La classe ``Result`` de base ne transporte pas d'autres informations qu'un booléen de succès ou d'échec. Elle doit idéalement servir de base à toute forme de résultat dans notre domaine. CQELight fourni cette classe de base ainsi que le class ``Result<T>`` qui permet d'encapsuler une information de retour. Vous restez libre de créer vos propres héritages de ``Result`` selon les besoins de vos domaines (dans l'exemple ci-dessus, on aurait pu créer une classe ``TransfertResult`` qui hérite de ``Result`` pour le cas précédent).

Commands
^^^^^^^^

Afin de rendre tout ceci possible, il est nécessaire d'exposer publiquement notre aggregat (pour que chacun puisse l'appeler selon son besoin), mais également de fournir une méthode de récupération d'un aggregat totalement reconstitué. L'utilisation d'une factory statique au niveau de l'aggregat reste possible, ce n'est pas forcément totalement optimal si le domaine est complexe (surtout qu'on arrive à une pollution du domaine avec des problématiques infrastructurelles). De la même manière, cela implique de laisser notre aggregat avec une visibilité publique générale, ainsi que potentiellement les objets dont il est composé, ce qui peut créer des problèmes de séparations en couches.

La notion de commande asynchrone de CQELight a été créée à cet effet. Elle est totalement dispensable dans les cas où vous devez interagir de façon synchrone avec votre domaine. Cependant, même si la logique d'instanciation/récupération de votre domaine est peu complexe, il est recommandé d'utiliser le pattern asynchrone de CQELight pour maximiser les performances et s'assurer d'un vrai découpage CQRS. Cela permet également de garder les objets domaines visibles uniquement à un niveau internal, évitant les raccourcis malheureux.

Une commande est la volonté d'un intervenant extérieur à intéragir avec le domaine concerné. Il s'agit donc d'un simple DTO, qui véhicule les informations nécessaires pour exécuter le code requis, et par son type/nom, envoie également l'information de l'action à effectuer. De façon générale, l'handler de la commande va considérer que s'il en récupère une, elle est considérée comme valide (il ne faut donc pas faire de vérification métier des paramètres de la commande dans le handler, on aurait une fuite des contrôles du domaine). La commande est responsable de ses données, il est recommandé de créer un constructeur avec vérification des paramètres.

.. note:: Attention, il n'est question ici que de vérification technique des paramètres (null, empty strings, ...), et non pas métier. Généralement, c'est le domaine qui va réagir en fonction des valeurs, mais un premier filtre peut déjà être effectué si nécessaire, éviter d'avoir à polluer notre domaine avec des contrôles techniques.

Une fois la commande créée, il est nécessaire de l'envoyer dans le système afin que le(s) handler(s) disponible(s) puisse(nt) y réagir. Pour effectuer cette action, on utilisera une instance du dispatcher qui se chargera de l'envoyer dans les bus nécessaires. Le rôle du handler est de s'occuper des problématiques d'infrastructure pour restaurer une instance d'aggregat (depuis la base de données, avec des événements, appel à une factory, ...).

.. note:: Même si on parlé de plusieurs handlers, il est fortement recommandé qu'il n'existe qu'un seul handler pour une commande donnée, et ce afin d'éviter plusieurs comportements inattendus (race-condition, deadlocks, accès concurrents, ...)

De même lorsque l'aggregat est restauré et que l'action du domaine est invoquée, il y a fort à parier qu'un résultat a été produit (événement, information de retour, ...). Le rôle du handler de commande sera également de s'occuper du traitement de ce retour (par exemple envoi des évènements par le biais du dispatcher). Encore une fois, si vous avez besoin d'un appel synchrone au domaine, mieux vaut se passer de ce fonctionnement asynchrone.

::

    using CQELight.Abstractions.CQS.Interfaces;
    public class ExecuteDomainAction : ICommand
    {
        //Some properties
    
        public ExecuteDomainAction() 
        { 
            //Execute some parameters checking here
        }
    }
	
::

    using CQELight.Abstractions.CQS.Interfaces;
    public class ExecuteDomainActionHandler : ICommandHandler<ExecuteDomainAction>
    {
    
        public Task HandleAsync(ExecuteDomainAction command, ICommandContext context = null);
        { 
            //Retrieve an instanciated aggregate
    
            //Execute domain action
    
            //Treat result of domain action
        }
    }
     
.. note:: Il est préférable d'éviter que nos handlers renvoient des exceptions car les bus n'ont peut-être pas de mécanisme traitement des exceptions particuliers, ce qui peut causer un crash ou une instabilité globale du système, voire une perte de l'information d'échec, menant à un comportement inattendu. Il est fortement recommandé d'éviter toute forme d'exception dans ces appels et traitements et d'encapsuler les traitements (récupération comme exécution niveau domaine) par des try-catch pour éviter ce genre de déconvenues.

Queries
^^^^^^^
A l'inverse de la commande qui est une volonté d'interagir avec le domaine et de le modifier, les queries permettent une récupération d'informations qui auront été générées par le domaine. Dans un logiciel de gestion classique, la majorité du temps passé à interagir avec la source de données se fera en lecture plutôt qu'en écriture. Ici, le concept de CQRS qui propose de séparer en deux piles différentes les lectures et les écritures prend tout son sens car le développeur restera libre d'implémenter différemment la pile des lectures pour l'optimiser.

De la même façon, les logiciels de gestion se contentent très rarement de travailler exclusivement avec une source de données volatile type mémoire vive, il y a toujours une forme de persistance. Lorsqu'on décide de persister les données, il faut garder en tête le pattern de fonctionnement pour de stocker les données de façon à ce que la lecture soit optimisée et indolore (quitte à dénormaliser à l'extrême) plutôt que d'essayer d'optimiser le stockage, ce qui ralentira les temps de traitements.

En résumé
^^^^^^^^^

Pour résumer, à un niveau aggregat, le pattern CQRS impose une distinction entre récupération de données et modification du système (Command Query Separation), tandis qu'à un niveau système, les commandes seront utilisées pour interagir globalement avec le domaine. Les handlers se chargent des problématiques globales d'infrastructures, laissant ainsi le domaine pur. Finalement, les queries permettent de récupérer des données qui auront été stockées de façon optimisée, afin de permettre un affichage optimal.