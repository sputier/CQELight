Modélisation du domaine
=======================
Généralités
^^^^^^^^^^^

Pour modéliser votre domaine selon le `Domain Driven Design <https://en.wikipedia.org/wiki/Domain-driven_design>`_ (DDD dans la suite de la documentation), il est nécessaire de réfléchir à son découpage, afin de déterminer ce qui est essentiel à votre métier, et ce qui est annexe. Ce découpage se fait sous forme de Bounded Context (contexte borné).

Ce découpage en contextes se fait selon une **logique métier**. Il est nécessaire de faire ce cloisonnement, et de définir à l'intérieur de chaque contexte un **langage unifié**, l'Ubiquitous Language. Ce langage définit le nom des objets et des procédures qui évoluent au sein du contexte. Ce langage utilise la terminologie du métier, c'est à dire que les termes doivent être compréhensibles par une personne non technique dans l'équipe qui alimente le backlog fonctionnel, ou même par les clients/utilisateurs.

Le schéma général du DDD est le suivant :

.. figure:: images/ddd-full-diagram.png

Comme on peut le constater sur ce schéma, l'ensemble de la philosophie DDD est complète, concernant autant l'organisation que la technique, et peut paraître complexe. Mais il n'en est rien. Il faut partir de l'élément principal=Model-Driven-Design.

Avant toutes choses, il faut éclaircir le concept. Le système informatique qui va être créé sera piloté par un modèle, qui sera lui-même piloté par le domaine (le métier). Donc, le métier est au centre de la pensée. En partant de ce point de départ, on constate que le schéma se découpe en deux parties distinctes.

La partie basse se concentre sur l'organisation du code et de l'équipe. Beaucoup de concepts y sont abordés et dépassent le cadre de CQELight. Si vous désirez en savoir plus sur cette organisation, nous vous invitons très vivement à vous tourner vers le net et la littérature sur le DDD, assez conséquente, ou vous tourner vers notre e-formation approfondie sur le DDD (à venir). La seule information a récupérer de ce bloc est la notion de séparation en 'Bounded Context'. Ici, chez Hybrid Technologies Solutions, pour marquer cette séparation, nous aimons faire une solution Visual Studio par contexte traité, afin d'être sûr de garantir le cloisonnement et l'indépendance.

Par contre, nous allons nous attarder sur la partie haute, car il s'agit à proprement parlé de la modélisation technique du domaine et ce qui tourne autour.

Architecture générale
^^^^^^^^^^^^^^^^^^^^^

Il est nécessaire d'architecturer un projet en couche ('Layered architecture') dans le schéma. C'est un concept très répandu, qui veut que le code soit découpé en plusieurs couches, une couche ne connaissant que la couche inférieure à elle, et exposant des informations à celle au-dessus d'elle, sans avoir connaissance de cette dernière. Il existe plusieurs architectures en couches, nous ne nous étendrons pas sur le sujet, mais chez Hybrid Technologies Solutions, on préconise une architecture décentralisée et découpée selon le mode CQRS, avec une encapsulation forte. Nos exemples montrent ce mode de fonctionnement, et vous pouvez également avoir différentes approches dans notre e-formation (à venir).

La couche qui nous intéresse ici est la couche dite domaine (ou business). C'est celle qui contient les objets représentant le métier et permettant de faire fonctionner le logiciel en adéquation avec les besoins métiers. Comme on peut le voir sur le schéma, il est nécessaire de découper un contexte en plusieurs entités ('Entities' sur le schéma). Ces entités représentent des blocs cohérents et consistants au sein d'un contexte. Elles véhiculent des données mais également un comportement en rapport avec ces données.

Cependant, une entité n'est pas la seule et unique représentation du contexte. On va également y trouver des ValueObjects, qui transportent une donnée immuable, au sens métier. Un ValueObject comporte une notion métier très forte.

ValueObjects
^^^^^^^^^^^^

La notion de ValueObject permet de transporter une valeur métier forte, qui est identifiée par l'unicité de chacun de ses membres à un niveau métier. Par exemple, si on modélise un système bancaire et qu'on veut utiliser la notion d'argent, on créera une classe qui permet d'encapsuler le type primitif C# ``decimal``, et d'y ajouter, si nécessaire, des informations supplémentaires (par exemple la devise).

Si on teste l'unicité d'un tel objet, on ne va pas le faire au niveau technique/système (comparaison de référence objet), mais on va le faire avec un point de vue métier. Dans notre précédent exemple, c'est l'égalité du montant et de la devise qui détermine si les deux sont égaux, et non les pointeurs vers la mémoire.

Un ValueObject n'est pas qu'un simple type complexe qui transporte des informations de façon immuable, il peut également transporter une ou plusieurs actions. Toujours d'après notre exemple, on pourrait ajouter dans notre ValueObject la possibilité de faire une addition. Le point important sera de garantir l'immuabilité afin d'éviter les effets de bord de son utilisation.

Du bon choix des types ...
^^^^^^^^^^^^^^^^^^^^^^^^^^

Lors du design d'un système, on tend naturellement à utiliser des types primitifs pour définir les valeurs de nos objets (``string``, ``int``, ``DateTime``, ...). Cela est bien, cependant, ils ne véhiculent aucune information métier forte ni n'assurent de sécurité à la compilation (par exemple, comment distinguer un string qui réprésente le nom de celui du prénom à la compilation ?).

Cet "effet de bord" nous force à repenser la définition des types de notre domaine en encapsulant les types primitifs dans des objets qui véhiculent non seulement une identité forte, mais assurent aussi une sécurité à la compilation ainsi qu'au développement (en ajoutant des vérifications à la construction par exemple).

Entities
^^^^^^^^

Une entité est un objet plus complexe, qui transporte des données et véhicule un comportement. Contrairement à un ValueObject, les données d'une entité sont muables (peuvent être modifiées), mais uniquement par les comportements internes qu'elle aura prévu. Ainsi, on évitera de laisser toutes les propriétés en visibilité publique (on favorisera la portée private ou internal), et on autorisera la modification de ces données que par le biais de fonctions bien définies qui feront les vérifications nécessaires.

La notion d'entité n'a véritablement pas de sens en tant qu'unité, elle doit faire partie d'un tout. On ne traite pas avec une entité de façon isolée, sinon, c'est qu'il s'agit d'un aggregat et non d'une entité. Concernant les données, celles-ci peuvent être diverses (ValueObject, autres entités, types primitifs) mais ne peuvent pas contenir une instance d'un aggregat (ni l'aggregat propriétaire, ni un autre aggregat du contexte).

Enfin, afin d'accentuer le métier, certains développeurs préfèreront mettre le constructeur privé et utiliser une méthode factory qui sera statique et qui portera le nommage propre au domaine afin de créer une nouvelle entité. Concernant l'implémentation comportementale et les données, il faudra coller au maximum aux contraintes du métier. Il est aussi possible d'utiliser la factory si la logique de construction est trop complexe pour être passée par un simple constructeur.

Aggregats
^^^^^^^^^

L'aggregat est le point d'entrée dans le métier afin de permettre une gestion cloisonnée du domaine. Si on est dans un système event-sourcé et CQRS, c'est également lui qui répondra aux Commands et propagera des Events. Il affiche donc une API publique et est responsable de la cohésion de son état interne. Il ne faut pas permettre de modifications de son contenu depuis l'extérieur car on pourrait arriver à un état inconsistant. Il faut considérer un aggregat comme un regroupement logique d'élements métiers définis au préalable.

.. note:: Attention cependant car il n'est pas nécessaire d'implémenter obligatoirement les autres types d'objets par aggregat. Ce qui est impérativement nécessaire par contre, c'est de garder toute mutation du domaine sous contrôle des fonctions de l'aggregat.

.. note:: Il n'est également pas grave d'avoir de multiples aggregats pour un contexte donné, si tant est que cela corresponde au besoin métier. De la même façon, il est préférable d'avoir une finesse de plusieurs petits aggregats (principe SOLID S) que de mettre tout au sein d'un seul et d'embarquer des données et un comportement inapproprié (souvent avec effet de bords).

Services
^^^^^^^^
La notion de services permet de définir des comportements qui ne nécessitent pas d'état mais véhiculent une notion métier. Dans le cas de notre exemple, on mettrait en service un système de conversion d'argent d'une monnaie vers une autre, car il y a un comportement métier fort, totalement indépendant et potentiellement complexe.

La définition d'un comportement au sein d'un service ou d'un objet métier (aggregat, entity ou ValueObject) reste soumise à l'appréciation du besoin métier. La règle principale c'est de savoir s'il est nécessaire d'avoir un état pour effectuer l'opération. Toujours avec l'exemple d'un système de change, si la notion de change est directement dans le système a été modélisé, l'opération aura un sens d'être implémentée dans un objet du domaine. A contrario, si l'information est fournie au moment de la transformation et n'est pas conservée car ce n'est pas le but du domaine, on choisira d'en faire un service.

Domain Events
^^^^^^^^^^^^^

Les événements sont une réaction du domaine à une sollicitation, notamment une modification. Ils sont importants même si le système n'utilise pas forcément une technologie de type Event-Sourcing. On peut utiliser un événement au lieu d'un type de retour car il transporte plus d'informations métier. De la même façon, il peut correspondre au résultat d'un appel sans forcément avoir besoin d'un système de type messaging.

L'événement domaine est une des notions les plus importantes dans l'ensemble du système car on véhicule un changement métier fort. Une des façons de modéliser un domaine peut par exemple être de rechercher l'ensemble des événements qui peuvent être générés (Event-Storming) et les découper conceptuellement pour les regrouper sous une même logique.

Factory et repository
^^^^^^^^^^^^^^^^^^^^^

Factory et repository sont deux patterns bien connus des développeurs. Ces deux patterns permettent la récupération d'objets métiers depuis une source de données externe (repository) ou depuis des données en mémoire (factory). On utilisera ces derniers si l'instanciation de nos objets est une procédure métier plus complexe qu'une simple initialisation (par exemple affectation de valeurs calculées ou génération d'objets).

L'implémentation de ces patterns reste à l'appréciation de chacun. Il n'est pas nécessaire d'avoir un objet pour pouvoir l'utiliser (parfois une simple méthode statique suffit). On aura d'ailleurs plutôt tendance à utiliser une factory pour les implémentations métier complexes afin de ne pas créer de couplage fort avec une source de données quelconque.