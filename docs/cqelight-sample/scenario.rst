Scénario complet
================

Afin d'explorer au maximum CQELight de façon assez simple, nous allons faire, dans un contexte mono-applicatif desktop console, un programme de test qui démontrera les concepts que nous avons vu précédemment.

L'idée va être de développer une petite application permettant la gestion d'un arbre généalogique, de façon ultra simplifiée. On se contentera uniquement de lister les personnes d'une famille, avec ses infos de naissance et leur date de décès.

Au niveau des informations de naissance, on se contente de stocker date et lieu de naissance. La date de décès peut ne pas être renseignée. Il est bien entendu impossible d'avoir une date de décès inférieure à la date de naissance. On considère à cet effet que deux personnes sont nées "de la même façon" si elles sont nées au même endroit le même jour.

La famille est identifiée de façon unique par le nom. Il ne peut pas y avoir deux familles avec le même nom. Au niveau des personnes, en plus des infos de naissance, on stockera uniquement le prénom. Il est possible d'avoir plusieurs personnes avec le même prénom dans la même famille, si les informations de naissance sont différentes, sinon, c'est qu'il s'agit d'un doublon, et ce n'est pas autorisé.

Dans les pages suivantes, on va explorer ce sujet petit à petit pour le modéliser et utiliser CQELight pour arriver à nos fins. Nous allons créer un système event-sourcé, séparé en CQRS et hautement extensible.

.. warning::Ce scénario n'est pas production ready, il est là pour montrer la façon d'utiliser l'outil et les bonnes pratiques associées. 

Vous trouverez l'ensemble du code sur `notre repository GitHub <https://github.com/cdie/CQELight/tree/master/samples/documentation/2.Geneao>`_.