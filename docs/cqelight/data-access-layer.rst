Accès aux données
=================

Dans les applications de gestion modernes, il est devenu obligatoire d'avoir accès une source de données durable (base de données, fichiers, ...). Cette obligation a donné lieu à la naissance de beaucoup d'outils divers et variés, dont par exemple les ORM. Cependant, il arrive parfois qu'au cours de la vie d'un logiciel, le type de source de données soit amenés à évoluer (passage de stockage sous forme de fichier en BDD par exemple).

A cet effet, il est impératif de commencer à penser son code pour que ce changement soit le plus anodin possible. Il faut créer une couche d'abstraction au dessus de l'accès au données, en utilisant le pattern **repository**. L'utilisation de ce pattern est repris dans le schéma de base du DDD, car l'accès aux données fait partie intégrante d'un logiciel métier.

CQELight fourni les abstractions de base d'un repository. En travaillant uniquement avec ces dernières dans votre code, vous serez libre, en un changement de ligne dans votre bootstrapper, de changer de source de données.

.. warning:: Il est tentant d'utiliser uniquement les abstractions et implémentations de base pour procéder à la gestion des données. En procédant de la sorte, vous vous évitez probablement du travail mais vous perdez en **testabilité** (les abstractions de bases sont testables uniquement dans une certaine limite), vous perdez en **visibilité** (les opérations utiliseront une terminologie technique et non métier) et vous perdez en **performance** (en utilisant les méthodes de base, vous n'avez pas la main sur toute l'API d'un ORM particulier par exemple, chose que vous pouvez optimisez en créant vos propres méthodes)

Nous conseillons de créer un repository par concept domaine persistable qui hérite de l'implémentation de base du repository de la technologie que vous avez choisi. Certes, cela oblige à redéfinir certaines choses en cas de changement de solution de persistence, mais vous vous évitez les problèmes cités précédemment.

Accès en lecture
^^^^^^^^^^^^^^^^

.. note:: Nous avons volontairement fait le choix de rendre les APIs de lecture uniquement asynchrone, car cela dépend d'une source externe dans la quasi-totalité des cas.

La lecture des données dans votre source est probablement l'opération que vous ferez le plus souvent. Il y a énormément de technique d'optimisation à ce niveau pour gagner en performance et en temps de traitement (cache, optimisation requêtage, désactivation du suivi des modifications, ...).

Afin de permettre la consultation, nous fournissons l'interface ``IDataReaderRepository<T>``. Cette interface expose deux méthodes de lecture : ``GetByIdAsync`` et ``GetAsync``. La récupération par identifiant permet de lire un élement uniquement sur la base de son identité, tandis que le ``Get`` permet de récupérer une collection répondant à certains critères.

La méthode ``GetAsync`` permet de spécifier en paramètre :

- Un filtre sous forme de prédicat auxquels les élements devront répondre afin d'être dans la collection de résultat
- Un ordre particulier, qui sera effectué côté serveur
- Un flag indiquant si on récupère les élements qui ont été marqués comme supprimés de façon logique
- Le ou les objets/collections liés à charger lors de la récupération. Cette option est utilisée dans le cadre de relation entre entités, et est donc reservée de façon quasi-exclusive aux SGDB relationnels

Cette méthode renvoie un ``IAsyncEnumerable`` qui peut-être itérée ou transformée de façon asynchrone, permettant une récupération et une évaluation des paramètres lors de la demande de récupération des données.

Accès en écriture
^^^^^^^^^^^^^^^^^

Pour avoir des données à lire, il faut d'abord en écrire. L'interface qui permet d'écrire les données est un peu plus complète, car elle offre une finesse de distinction entre l'ajout et la modification. La majorité des méthodes de cette inferface permettent donc d'appliquer un marquage sur les entités afin que lorsque la transaction sera marquée comme complétée (par le biais de la méthode ``SaveAsync``), l'opération soit réalisée de façon atomique selon ce qui a été décidé auparavant.

.. note:: L'appel à ``SaveAsync`` permet aux classes repository enfants de gérer la complétion de sa propre transaction métier (pattern Unit of Work), chose qui n'aurait pas été aisément réalisable si les méthodes Insert ou Update avaient fait l'enregistrement directement.

Chaque méthode d'écriture propose une version unitaire et une version multiple (ex : ``MarkForInsert`` et ``MarkForInsertRange``). La suppression est également possible uniquement par le biais de l'ID, et ce afin d'éviter à avoir à procéder à un chargement de l'entité pour uniquement la supprimer. La gestion de la suppression permet l'utilisation d'un mode physique (la ligne est supprimée en base de données) ou d'un mode logique (la ligne est modifiée avec un flag qui l'indique comme supprimé). En cas de suppression logique, on peut indiquer lors de nos ``GetAsync`` si l'on veut remonter les enregistrements ou non, chose impossible en fonctionnement physique.

.. note:: Dans un fonctionnement CQRS-EventSourcing, les données remontées par les repositories seront des transformations d'évènements optimisés pour la lecture, l'utilisation de la suppression logique est contre productif car les vues ne sont pas la source de vérité. Il faudra bien penser à activer la suppression physique, désactivée par défaut, durant les appels. Il est possible de définir ce comportement par défaut lors des développements de vos plugins et d'ignorer les paramètres des fonctions. Les plugins CQELight officiels donnent la possibilité de préciser ce comportement lors du bootstrapp.

Spécificités BDD relationnelle (SQL)
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Malgré que le nombre de source de données soit conséquent, le monde des bases de données relationnelles ne peut pas être ignorés. A cet effet, une interface dédiée à ce type de source a été ajoutée afin de permettre d'utiliser leurs spécificités (exécution de code SQL). De façon générale, il est fortement recommandé de ne pas exécuter du code SQL directement dans le code applicatif mais de passer par des méthodes de transformation. Certains cas, cependant, peuvent nécessiter d'utiliser l'API SQL directement. Il suffira d'utiliser l'interface ``ISqlRepository``.

L'interface ``ISqlRepository`` fourni les méthodes à cet effet, permet l'utilisation du SQL directement sur la base de données. Les méthodes ne permettent pas de récupération de collection de données, uniquement de faire une modification ou de récupérer une valeur scalaire unitaire, ceci afin de décourager l'utilisation de ces APIs de façon trop régulière.

Intégration dans un système CQRS
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Le pattern repository ainsi que les abstractions (et les implémentations fournies) sont suffisantes pour faire un système fonctionnel. Cependant, dans le cadre de la méthodologie CQRS, il est préférable de créer une couche Query, qui utilise les repository afin d'obtenir les données, et d'utiliser une système de cache.

Si vous utilisez également les domain-events (avec ou sans Event Sourcing), il est également conseillé de faire de l'invalidation de cache à l'aide des évènements. Tous ces concepts sont avancés et sont expliqués et fournis à titre d'exemple dans les documentations associés ainsi que les exemples disponibles sur `GitHub <https://github.com/cdie/CQELight/tree/master/samples>`_.