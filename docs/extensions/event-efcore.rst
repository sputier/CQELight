CQELight : qu'est ce que c'est ?
================================
La première question à se poser est : qu'est-ce qu'un logiciel ? Une réponse à cette question est :

    *Un logiciel est un outil permettant de résoudre des problèmes et d'ajouter un plus grand confort de travail pour un ou plusieurs métiers d'une entreprise*

De fait, si on considère cette définition, on retrouve un point très important, **la notion de métier**. En effet, généralement, les développeurs se focalisent sur l'aspect technique d'un logiciel, demandant à un product owner ou à d'autres personnes maîtrisant le métier, comment implémenter telle ou telle fonctionnalité (si ce n'a pas déjà été défini dans un cahier des charges) et tentent de faire rentrer le métier dans la technique qui a été mise en place.

Très souvent on constate qu'un logiciel qui a été créé il y a plus de deux ans devient plus lent, plus lourd, plus compliqué à maintenir et les adaptations à implémenter pour s'accorder à l'évolution du métier auquel il répond sont de plus en plus complexes et risquées. Le terme legacy est par ailleurs souvent utilisé par les développeurs pour déterminer ce genre de situation, et on arrive plus difficilement à trouver des personnes motivées pour en faire la maintenance.

**CQELight** n'est pas un outil magique qui fera que des foules se presseront pour faire de la maintenance sur l'applicatif qui sera construit avec lui. Cependant, il apportera un ensemble d'outils, de patterns et de bonnes pratiques permettant de simplifier, voire parfois totalement éliminer certaines parties de la maintenance nécessaire.

A l'instar de beaucoup de framework, CQELight ne contient que les briques de base permettant la construction d'un logiciel. C'est lui qui va se charger de la grande partie des problématiques infrastructurelles et architecturales pour que les développeurs puissent se focaliser sur l'implémentation du métier. L'avantage de cette vision des choses : si le logiciel est construit en se basant sur le métier au lieu de se focaliser sur la technique, il pourra plus facilement suivre les évolutions du business, et même permettre à de nouveaux entrants sur le projet d'apprendre le métier en parcourant le code.

Quels sont donc les outils que CQELight met à disposition ? On peut en sortir une liste facilement, sachant que le concept de base est l'extensibilité et l'adaptabilité aux pratiques et outils existants :

- Gestion de la séparation du code en Command et Query
- Gestion de l'envoi/réception des Commands et Events
- Objets de base pour le modeling métier
- Configuration fine du comportement du système
- Gestion simplifiée de l'injection de dépendance
- Gestion simplifiée des accès aux données
- Maintenance évenementielle assistée

Il n'est pas impossible que certaines de ces notions ne vous parlent pas spécialement. Le but de cette documentation est de vous éclairer sur ces notions, et vous donner les informations pour les utiliser dans vos implémentations, avec l'aide de CQELight.

C'est parti, commençons par le :doc:`getting-started`, pour savoir comment débuter facilement et rapidement !