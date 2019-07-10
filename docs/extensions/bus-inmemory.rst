Bus In-Memory
=============

Le bus In-Memory fais partie du coeur du système de CQELight. C'est lui qui fait transiter la totalité des messages (événements et commandes) d'un point à un autre. C'est aussi lui qui se place en sortie des autres bus afin d'envoyer les informations dans le système courant. Bien qu'il soit disponible sous forme de package, il y a fort à parier qu'il soit un élement central et indispensable de votre futur applicatif. Le bus In-Memory fonctionne sur un principe simple : il scanne les assemblies de votre projet pour y trouver l'ensemble des handlers de destination de l'information qu'il a à envoyer.

Comme une grande partie de nos composants, le bus In-Memory est hautement configurable pour faire face aux besoins que vous pourriez avoir. Afin de l'utiliser, il suffit simplement d'appeler la méthode d'extension ``UseInMemoryEventBus`` et/ou ``UseInMemoryCommandBus``. Les deux méthodes ont la même logique d'appel : soit on fourni un instance de la classe d'options, soit on configure le composant à l'aide d'une lambda. Pour finir, on peut également préciser une collection de noms d'assemblies à exclure pour la recherches d'handlers afin d'améliorer les performances.

A noter cependant que ces paramètres de personnalisation sont totalement optionnels et l'on peut se contenter des appels les plus simples possibles que sont ``myBootstrapper.UseInMemoryEventBus()`` et ``myBootstrapper.UserInMemoryCommandBus()``.

::

    //without options
    myBootstrapper.UseInMemoryEventBus();
    //with options
    myBootstrapper.UseInMemoryEventBus(opt => { opt.NbRetries = 3; });

Etant donné que l'envoi se fait en mémoire, il peut être nécessaire d'avoir envie d'ordonner ou de gérer la priorité de réception. A cet effet, un attribut est disponible : ``HandlerPriority``. Il suffit de le mettre au dessus de la déclaration d'une classe, avec la valeur désirée, pour que cet handler soit placé comme il se doit dans la liste des envois.
	
..warning :: Il n'est pas possible de garantir la priorité d'un handler par rapport à un autre si les deux possède la même valeur dans l'attribut. De même, cette valeur n'est pas considérée en cas de gestion en parallèle.

De la même manière, il peut être parfois nécessaire, dans votre solution, de s'assurer qu'un handler soit exécuté avec succès avant de passer aux autres. Ces handlers sont qualifiés de "critiques", et doivent donc être marqués avec l'attribut CriticalHandler.

..warning :: Cette notion de criticité n'est valide que si l'appel des handlers est fait de façon procédurale, c'est à dire un après l'autre. Cette valeur n'est pas considérée en cas de gestion en parallèle.

Lorsque votre projet commencera a devenir conséquent, il y a fort à parier que vous n'aurez pas ou plus la vision globale de la configuration. Afin de palier à cela, le bootstrapping renvoie une collection de notification vous aidant à voir s'il y a des failles dans votre configuration. Ces notifications sont soumises au flags 'Strict' et 'Optimal' du bootstrapper. La totalité des notifications sont des warning, il est donc nécessaire d'être vigilant et de surveiller le retour de la fonction Bootstrapp.

Plusieurs options sont à votre portée pour configurer le bus In-Memory d'événements :

- ``WaitingTimeMilliseconds`` : temps d'attente entre deux essais de livraison d'événement, en cas d'échec lors de la première tenative.
- ``NbRetries`` : nombre d'essai lorsqu'une livraison d'événement n'a pas pu se dérouler comme prévu.
- ``OnFailedDelivery`` : callback invoqué avec l'événement et le contexte associé lorsque ce dernier n'a pas pu arriver convenablement à destination.
- ``IfClauses`` : conditions particulières indiquant si l'événement doit être envoyé dans le système ou non.
- ``ParallelHandling`` : flag par type d'événement autorisant le système à gérer le handling parallèle d'une instance du type d'événement configuré. Attention : incompatible avec les attibruts de priorité et de criticité.
- ``ParallelDispatch`` : flag par type d'événement autorisant le système à propager l'événement en parlalèle dans le système.

Il y a également quelques options pour la configuration du bus In-Memory de commandes :

- ``OnNoHandlerFounds`` : callback invoqué avec la commande et le contexte associé en cas d'absence de handler pour la traiter.
- ``IfClauses`` : conditions particulières indiquant si la commande doit être envoyée dans le système ou non.
- ``CommandAllowMultipleHandlers`` : configuration pour un type de commande particulier afin d'indiquer plusieurs handlers sont autorisés ou non.