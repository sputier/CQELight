Modélisation du domaine
=======================

Avant toute chose, il convient de modéliser le domain métier par les objets le représentant. Cette modélisation est volontairement simple et couvre explicitement les trois objets de base (Aggregate, Entity et ValueObject).

ValueObject
^^^^^^^^^^^

Afin de pouvoir stocker des valeurs et effectuer des traitements, nous allons découper notre domaine dans ces trois types d'objet. Pour rappel, un ValueObject (VO dans la suite du texte) est identifié par l'unicité métier de l'ensemble de ses membres et doit être immuable. La notion qui colle le plus à une VO dans notre exemple est la gestion des informations de naissance (date et lieu), étant donné que le scénario considère que deux personnes sont nées de la même façon si elles sont nées le même jour au même endroit.

Le VO qui en découle est le suivant ::

    using CQELight.Abstractions.DDD;
    using System;
    
    namespace Geneao
    {
        public class InfosNaissance : ValueObject<InfosNaissance>
        {
    
            public string Lieu { get; private set; }
            public DateTime DateNaissance { get; private set; }
    
            public InfosNaissance(string lieu, DateTime dateNaissance)
            {
                if (string.IsNullOrWhiteSpace(lieu))
                {
                    throw new ArgumentException("InfosNaissance.Ctor() : Lieu requis.", nameof(lieu));
                }
    
                Lieu = lieu;
                DateNaissance = dateNaissance;
            }
    
            protected override bool EqualsCore(InfosNaissance other)
            => other.DateNaissance == DateNaissance && other.Lieu == Lieu;
    
            protected override int GetHashCodeCore()
            => (typeof(InfosNaissance).AssemblyQualifiedName + DateNaissance + Lieu).GetHashCode();
        }
    }
         
   
     
Comme on peut le constater, un VO doit hériter de la classe ``CQELight.Abstractions.DDD.ValueObject`` pour avoir le comportement de base. Deux méthodes sont à overrider : ``EqualsCore`` et ``GetHashCodeCore``, afin de permettre de redéfinir niveau métier ce qui fait foi pour cet objet. Afin de garantir son immuabilité, vous remarquerez que les setter sont privés et uniquement le constructeur peut définir ces valeurs. C'est voulu, c'est afin qu'il n'y ait aucun changement au cours de la vie d'un objet. De façon générale, lors de la création d'un VO et son stockage auprès d'une entité, on s'assure effectivement que la même valeur soit véhiculée. Si nous avons besoin de changer une information, on créera une nouvelle instance.

Du bon choix des types
^^^^^^^^^^^^^^^^^^^^^^

Dans notre petit domaine, nous allons utiliser un Guid pour définir une personne et un string pour identifier une famille. Il est nécessaire de ne pas utiliser les types primitifs car cela n'a aucun sens métier fort. Au lieu de ça, il est préférable de les encapsuler pour leur donner une vraie définition ::        

    using System;
    
    namespace Geneao.Identity
    {
        public struct PersonneId
        {
            public Guid Value { get; private set; }
    
            public PersonneId(Guid value)
            {
                if (value == Guid.Empty)
                    throw new InvalidOperationException("PersonneId.ctor() : Un identifiant valide doit être fourni.");
                Value = value;
            }
    
            public static PersonneId Generate()
                => new PersonneId(Guid.NewGuid());
        }
    }
	
::

    using System;
    
    namespace Geneao.Identity
    {
        public struct NomFamille
        {
            public string Value { get; private set; }
            
            public NomFamille(string value)
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
                    throw new InvalidOperationException("NomFamille.ctor() : Un nom de famille correct doit être fourni (non vide et inférieur à 128 caractères).");
                Value = value;
            }
            public static implicit operator NomFamille(string nom)
                => new NomFamille(nom);
        }
    }
	
Ces deux classes nous permettent d'utiliser, dans l'ensemble de notre modèle, un identifiant unique pour une Personne et pour une Famille. L'intérêt d'avoir encapsulé les types simples dans ces classes d'identité nous permet de respecter le principe Single Responsability (pour la vérification de la donnée), et nous évitera ultérieurement des erreurs de code (comme par exemple, dans un méthode, avec un string pour le nom et un string pour le prénom, il est très facile d'intervertir les paramètres, alors que si l'un de ceux-ci est un ``NomFamille``, on évite ce désagrément).

.. note:: Dans le code précédent, nous avons rajouté un opérateur implicite de conversion entre un string et la classe NomFamille. Le but de cette opération est de faciliter le développement en faisant ``NomFamille nom="famille1"``. Cependant, l'effet négatif de ce changement peut être l'inversion de paramètre dont nous parlions précédemment.

Entity
^^^^^^

Pour rappel, une entité véhicule des données (muable) et un comportement. Une entité n'est pas censée exister en dehors d'un agrégat donné. Dans notre gestion de famille, la notion de personne s'y porte le plus, car dans ce contexte uniquement, une personne n'est pas censée exister en dehors d'une famille.

.. note:: Attention, il est toujours nécessaire, quand on modélise le domaine, de rester concentré sur le contexte qu'on est entrain d'étudier et ne pas penser au système en général. Plusieurs entités autour d'un même objet réel vont être modélisées différement dans les différents contextes. Si une ou plusieurs entités seraient totalement identiques d'un contexte à l'autre, on peut alors parler de Shared Kernel, c'est à dire d'informations communes partagées car véhiculant un sens métier "universel" dans notre système.

::

    using CQELight.Abstractions.DDD;
    using System;
    
    namespace Geneao
    {
        public class Personne : Entity<PersonneId>
        {
            public string Prenom { get; internal set; }
            public InfosNaissance InfosNaissance { get; internal set; }
    
            internal Personne() { }
    
            public static Result DeclarerNaissance(string prenom, InfosNaissance infosNaissance)
            {
               if (string.IsNullOrWhiteSpace(prenom))
               {
                   return Result.Fail(DeclarationNaissanceImpossibleCar.AbsenceDePrenom);
               }
    
               if (infosNaissance == null)
               {
                   return Result.Fail(DeclarationNaissanceImpossibleCar.AbsenceInformationNaissance);
               }
    
               return Result.Ok(new Personne(PersonneId.Generate())
               {
                   Prenom = prenom,
                   InfosNaissance = infosNaissance
               });
            }
        }
    }
       
.. note:: Dans le bloc précédent, nous avons fait le choix d'utiliser la structure Result fournie avec CQELight au lieu des exceptions, et ce afin d'éviter de faire rentrer la mécanique de gestion des exceptions, qui peut être lourde en terme de performances. De plus, ``DeclarerNaissance`` est une fonction métier, elle a donc du sens à retourner un résultat métier plutôt qu'un résultat technique.

Le code est assez explicite pour décrire le comportement de cette entité. Ici, on considère la clé comme étant un ``PersonneId``. Ceci est fait en héritant de la classe ``CQELight.Abstractions.DDD.Entity`` avec l'id désiré. Ensuite, on fait en sorte que les données soit visibles de l'extérieur, mais modifiable uniquement de l'intérieur de l'assembly (pour que l'agrégat puisse les modifier si nécessaire). Finalement, on rends le constructeur internal (encore une fois pour les besoins éventuels de l'agrégat) et on fait une factory qui a un sens métier fort, avec les contrôles associés.

.. note:: Ici, on pourrait faire un raccourci rapide et considérer que c'est la responsabilité de l'agrégat de s'assurer que la création d'une personne est validée par lui seul. Nous préférons découper notre domaine de telle façon que chaque classe gère le contrôle de ses données propres, avec la possibilité pour l'agrégat parent d'en faire des modifications si nécessaire.

Aggregate
^^^^^^^^^

Pour rappel, la notion d'AggregateRoot est la partie publique de notre contexte courant, elle doit représenter une frange du métier de ce contexte. C'est donc l'objet qui sera désigné comme ``AggregateRoot`` qui exposera publiquement les moyens d'entrer en contact avec le domaine (modification ou lecture). De la même façon, un agrégat étant un regroupement métier, il est essentiel qu'il soit le garant de son état interne.

Nous allons gérer des personnes regroupées en famille plutôt que des personnes de façon individuelle. Une famille, dans ce contexte, regroupe un ensemble de personne, c'est donc notre AggregateRoot ::       

    using CQELight.Abstractions;
    using CQELight.Abstractions.DDD;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace Geneao
    {
        public class Famille : AggregateRoot<NomFamille>
        {
            public IEnumerable Personnes => _state.Personnes.AsEnumerable();
    
            private FamilleState _state;
    
            private class FamilleState : AggregateState
            {
    
                public List Personnes { get; set; }
    
                public FamilleState()
                {
                    Personnes = new List();
                }
             }
    
             public Famille(NomFamille nomFamille, IEnumerable personnes = null)
             {
                 Id = nomFamille;
                 _state = new FamilleState
                 {
                     Personnes = (personnes ?? Enumerable.Empty()).ToList()
                 };
             }
             
             public static Result CreerFamille(string nom, IEnumerable personnes = null)
             {
                 return Result.Ok(new Famille(new NomFamille(nom), personnes));
             }
    
             public Result AjouterPersonne(string prenom, InfosNaissance infosNaissance)
             {
                 if(!_state.Personnes.Any(p => p.Prenom == prenom && p.InfosNaissance == infosNaissance))
                 {
                    _state.Personnes.Add(Personne.DeclarerNaissance(prenom, infosNaissance));
                 }
                 return Result.Ok()
             } 
        }
    }
         
Notre agrégat est censé gérer la cohérence d'une famille, dans ce domaine. De fait, il est nécessaire de vérifier que la personne qu'on tente d'ajouter n'existe pas déjà dans cette famille. La factory de création permet, depuis un nom et une liste de personne, de récupérer un agrégat domaine de ``Famille``. A noter ici que si le nom est incorrect, la vérification est faite par la partie domaine de l'identité et renvoie l'exception directement, sans traitement. Ce comportement sera géré correctement lors de la mise en place des évènements domaine.

.. note:: C'est normalement ici qu'on fait la gestion des évènements. Nous prendrons cet exemple pour en parler ultérieurement dans la partie de la documentation sur les events & les commands.

Avec cette mise en place initiale, notre modèle est constitué et on peut continuer à l'enrichir selon l'évolution du métier. Dans notre cas, par exemple, et à titre d'exercice, on peut rajouter la notion de mariage, la notion d'enfant/parent, etc... Libre à vous de continuer sur cette lancée et de continuer cet exercice !

.. note:: Important : il faut garder en tête qu'une modélisation est toujours imparfaite. De ce fait, nous serons amenés tout au long de cet exercice à retoucher ce code. Il ne faut s'y "attacher" au point de vouloir le laisser tel quel. D'autre part, nous n'avons créé aucun test unitaire, c'est un bon exercice de créer des tests pour vérifier le code existant et s'assurer que nos prochains changements ne casseront pas le domaine.