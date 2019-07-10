Mapping modèle de données
=========================

Création du mapping du modèle
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Avant de pouvoir utiliser nos modèles avec un repository, il faut créer une couche de mapping, qui sera ultérieurement utilisée dans les plugins pour savoir comment gérer les entités dans une source de données. Pour cela, CQELight mets à disposition un ensemble d'attributs à appliquer à vos classes de données. Il reste possible de persister directement les objets du domaine dans un repository, mais en le faisant sans passer par un modèle dédié intermédiaire, il pourrait y avoir une pollution du domaine avec des problématiques de persistance (constructeur sans paramètre, visibilité publique, ...).

Pour éviter ce problème, il a été créé trois objets permettant de faire le lien entre une source de données et nos objets domaines : ``PersistableEntity``, ``ComposedKeyPersistableEntity`` et ``CustomKeyPersistableEntity``. Ces trois classes abstraites héritent de la classe abstraite de base ``BasePersistableEntity`` qui contient les élements communs, à savoir les valeurs de modification et de suppression. A un niveau d'abstraction supérieur, on retrouvera l'interface globale commune, ``IPersistableEntity``, qui permet à l'équipe de développement de créer ses propres entités persistables.

La création du modèle consiste a créer l'ensemble des objets qui seront persistés dans la source de données choisies. Il n'y a pas beaucoup de règles à suivre, si ce n'est de suivre les recommandations du provider d'accès aux données qui aura été choisi par le biais du plugin.

Utilisation des attributs
^^^^^^^^^^^^^^^^^^^^^^^^^

Cependant, afin d'offrir une certaine flexibilité dans la création de ce modèle, il faut permettre une certaine personnalisation du modèle, comme par exemple le nom de la colonne ou de la table qui sera utilisée en cas d'utilisation de SGBD relationnel. Tous ces attributs sont disponibles dans le namespace ``CQELight.DAL.Attributes``.

.. note:: Bien que certains ORM comme Entity Framework offrent déjà des attributs pour faire le mapping des modèles, nous avons souhaité redéfinir les notre afin de ne pas dépendre d'un provider particulier. Ainsi, il peut sembler parfois qu'il y ait un "double emploi", mais c'est afin de permettre de réaliser une seule fois le mapping et d'être compatible avec n'importe quel provider. Une attention particulière est donc portée à l'attention de l'équipe réalisant les mappings d'utiliser les attributs de CQELight (ou des assemblies qui sont communes, comme ``System.ComponentModel.Annotations``) au lieu d'attributs spécifiques d'un provider, et ce pour éviter de se retrouver bloqué sur une seule technologie de persistance.

Les attributs disponibles pour la création du modèle sont :

- **TableAttribute** : permet de définir le nom de la table dans lequel l'entité doit être stockée. Cet attribut permet de spécifier un nom de table et un nom de schema (spécificité SQL Server).
- **ColumnAttribute** : permet de définir le nom de la colonne dans laquelle la propriété de la donnée doit être stockée. Cet attribut permet de spécifier le nom de la colonne.
- **PrimaryKeyAttribute** : permet de définir sur une propriété d'une entité laquelle sert de clé primaire (valeur d'identification unique et unitaire d'une entité). Cet attribut permet de spécifier le nom de la colonne clé.
- **ComposedKeyAttribute** : permet de définir sur une entité l'ensemble des propriétés définissant la clé primaire composée (valeur d'identification unique composée de l'unicité de l'ensemble des propriétés choisies). Cet attribut permet de spécifier le nom des propriétés à utiliser pour la définition de la clé composée.
- **IgnoreAttribute** : permet d'ignorer une propriété dans l'élaboration du modèle.

Les attributs disponibles pour l'optimisation du SGBD sont les suivants :

- **IndexAttribute** : permet de définir un index sur une propriété. Cet attribut permet de définir un nom d'index et le fait que l'index doit respecter une clause d'unicité.
- **ComplexIndexAttribute** : permet de définir un index composé sur plusieurs propriété. Cet attribut permet de définir le nom de l'index et le fait que l'index doit respecter une clause d'unicité.
- **NotNaviguableAttribute** : permet de définir les propriétés qui ne doivent pas être parcourue lors du traitement en profondeur. Certains ORM (comme Entity Framework) parcoure systématiquement la grappe d'objets pour définir leur état. Cet attribut permet de bloquer ce parcours.

Les attributs disponibles pour la gestion des relations sont les suivants :

- **ForeignKeyAttribute** : permet de définir sur une propriété "objet" qu'il s'agit d'une clé étrangère. Cet attribut permet de définir le nom de la propriété dans l'objet distant (en cas d'existance de plusieurs relations) et permet aussi de définir le comportement à suivre en cas de suppression.
- **KeyStorageOfAttribute** : permet de définir qu'une propriété heberge la valeur clé étrange d'un objet défini par un attribut ForeignKey. Cet attribut permet de prendre en paramètre le nom de la propriété objet clé étrangère.

.. note:: La gestion des collections doit se faire idéalement dans les deux sens, à savoir une propriété de départ et une propriété d'arrivée (pour le 1-1 ou le 1-Many) et deux propriétés : une pour l'objet afin de naviguer et une (ou plus en cas de clé composée) pour la valeur clé de l'objet. Le type a utiliser pour les collections dans le 1-Many doit être de type ICollection<T> et ce afin d'être générique avec la totalité des providers disponibles sur le marché. La simple existence d'une propriété ICollection détermine l'existence d'une relation 1-Many. Il faut dès lors utiliser les attributs ci-dessus sur l'objet "maître" de la relation.

.. note:: Au jour d'aujourd'hui, il n'y a pas la possibilité de faire nativement une relation Many-to-Many, il est nécessaire d'utiliser un objet de transition.

Si le plugin du provider que vous avez choisi le supporte, vous pouvez également utiliser des attributs généraux issus du framework .NET (comme par exemple MaxLengthAttribute). Afin de savoir si c'est supporté, rendez-vous sur la documentation du plugin en question. Les implémentations de provider DAL doivent à minima supporter les attributs énoncés ci-dessus. Si vous avez besoin d'un exemple de mapping qui couvre l'intégralité des cas ci-dessus, `rendez-vous sur la classe contenant les entités utilisées pour les tests unitaires <https://github.com/cdie/CQELight/blob/master/tests/CQELight.DAL.EFCore.Integration.Tests/DbEntities.cs>`_ sur le provider DAL Entity Framework Core.