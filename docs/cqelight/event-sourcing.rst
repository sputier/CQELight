Event Sourcing
==============
Généralités
^^^^^^^^^^^
En event-sourcing, on considère les évenèments comme l'unique source de vérité. De ce fait, chacun des évènement est sauvegardé de façon durable, afin d'être réutilisé ultérieurement, notamment pour la prise de décision.

En effet, de façon très classique en développement informatique, il arrive assez souvent qu'on récupère des informations depuis une source de données afin de donner tout ce qu'il faut pour que l'utilisateur puisse effectuer une action en toute connaissance de cause. Généralement, on aura créé une base de données qui représente un instantané d'une situation, et on ignore (ou tout du moins on ne connait pas en détails) comment on est arrivé à cette représentation (même s'il arrive parfois qu'on créé des tables historiques ou qu'on profite de la flexibilité de certaines fonctionnalités de la base pour sauvegarder les modifications de données).

Cependant, en suivant la logique CQRS, la base de données doit être optimisée pour la lecture, quitte à dupliquer de la donnée. On risque dès lors de se retrouver avec du bruit, des informations non nécessaires, ou pire encore, des données qui n'ont pas été rafraichies et ne sont plus pertinentes, sur lesquelles l'utilisateur prendrait une décision qui ne serait, de fait, pas pertinente. Il serait également nécessaire de récupérer les données depuis de multiples sources, rendant possiblement le système contre performant. La seule solution pour prendre une bonne décision est de recréer l'état dans lequel était le système en prenant compte tout ce qui s'est passé. On appelle ceci **la réhydratation à base d'évènements**.

Modification du domaine
^^^^^^^^^^^^^^^^^^^^^^^
CQELight fourni des outils pour faciliter ce processus, notamment la notion d'event store. Comme son nom l'indique, un event store permet de stocker les évènements, avec une gestion tant en écriture qu'en lecture et une automatisation des routines. Il s'agit d'une extension, il vous faudra dès lors installer le provider qui correspond à votre stack technologique.

Bien que le comportement soit spécifiquement implémenté dans les extensions, certains concepts sont communs. Tout d'abord, pour commencer, les évènements viennent réhydrater un aggregat, de la même façon que c'est lui qui génère les évènements. Sauf que l'ensemble des développeurs qui veulent utiliser le DDD pour modéliser leur domaine n'a peut-être pas envie de faire un système event-sourcé. A cet effet, il faut explicitement définir son aggregat comme étant utilisé dans un système event-sourcé, en héritant de la classe ``EventSourcedAggregate<T>`` ::

    public class MyEventSourcedAggregate : EventSourcedAggregate
    {
        // Aggregate implementation
    }

.. note:: Il n'y a pas beaucoup de différences entre un ``AggregateRoot`` et un ``EventSourcedAggregate``, l'essence fonctionnelle reste la même. La seule différence réside dans le fait que l'aggregat doit pouvoir exporter un état sérialisé. Attention, cela ne veut en aucun cas dire que l'état doit être public, il suffit juste de pouvoir l'exporter de façon sérialisée afin de le sauvegarder dans l'event store si nécessaire.

L'aggregat doit également avoir un état qui doit pouvoir être muté selon les évènements qui sont arrivés. Cette notion d'état est quelque chose qui existe déjà en DDD, mais qui doit être approfondi en event-sourcing. A cet effet, une classe de base, ``AggregateState``, disponible dans le namespace ``CQELight.Abstractions.DDD`` mets à disposition les premiers éléments pour permettre la réhydratation, à savoir la possibilité d'ajouter les callback d'application de évènements pour muter l'état, et la possibilité de se sérialiser. Bien entendu, comme une grand majorité des choses dans CQELight, ces méthodes peuvent être overridées par vos implémentations si cela s'avère nécessaire.

Event store
^^^^^^^^^^^

CQELight propose des extensions implémentant des event store selon les abstractions fournies dans l'assembly de base. Les abstractions à implémenter se trouve dans le namespace ``CQELight.Abstractions.EventStore.Interfaces``

- ``IEventStore`` : C'est l'interface principale, le coeur du système d'event sourcing. L'event store doit définir les fonctions de récupération et de lecture des évènements tout comme la fonction d'écriture. A noter qu'une implémentation de cette interface est suffisante pour faire un système event sourcé où tout serait géré à la main, sans l'automatisation de CQELight, en gardant des objets métiers standards.
- ``IAggregateEventStore`` : Il s'agit d'une interface permettant de récupérer de façon plus automatisée les aggregats event sourcé totalement réhydratés. Les implémentations prennent en charge les problématiques de réhydratation, comme par exemple l'utilisation d'un snapshot comme base de travail.
- ``ISnapshotBehavior`` : Interface de contrat permettant de gérer la notion de snapshot pour des raisons de perfomances et de stockage.

CQELight mets à disposition deux providers d'event store qui proposent des implémentations pour ces abstractions, `CQELight.EventStore.EFCore <https://www.nuget.org/packages/CQELight.EventStore.EFCore/>`_ et `CQELight.EventStore.MongoDb <https://www.nuget.org/packages/CQELight.EventStore.MongoDb/>`_.