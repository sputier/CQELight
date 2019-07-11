Events
======

Nous avons vu comment solliciter le système pour que ce dernier entreprenne une action. Maintenant, il est nécessaire que le système réponde. En effet, nous avons envoyé la commande dans le système, et à part en debug en mettant un point d'arrêt, on a aucun retour du système, pour savoir si ça c'est bien ou mal passé, quand ça s'est terminé, etc... C'est ici que les événements domaine entre en jeu.

Un événement est généré par un agrégat, qui le stocke dans sa liste interne suite à une ou plusieurs action. Une fois ses traitements terminés, on peut lui demander de propager ses événements dans le système. En premier lieu, il faut donc créer l'événement :
::

    using CQELight.Abstractions.Events;
    using Geneao.Identity;
    
    namespace Geneao.Events
    {
        public sealed class FamilleCreeeEvent : BaseDomainEvent
        {
            public NomFamille NomFamille { get; private set; }
    
            private FamilleCreeeEvent() { }
    
            internal FamilleCreeeEvent(NomFamille nomFamille)
            {
                NomFamille = nomFamille;
            }
    
        }
    }
     
La classe d'événement contient dans notre cas plus ou moins les mêmes infos que la classe de commande. Ceci s'explique par le fait que notre système, dans le cas présent, ne génère ni ne transforme aucune information (si ce n'est la conversion d'un string en ``NomFamille``). Par contre, il est nécessaire de restituer les mêmes infos que la commande, ou tout du moins les infos nécessaire pour remettre le système dans un état équivalent, pour l'Event Sourcing, comme nous allons le voir par la suite.

De la même façon que pour la commande, la classe d'événement doit être sealed. Elle n'expose d'ailleurs aucun constructeur publique, car un évènement n'est envoyé que d'un et un seul contexte, mais peut être reçu par plusieurs. Evidemment, il est toujours possible de faire de la reflection pour contourner le système, mais l'idée est d'éviter les erreurs de développeurs honnêtes. Le(s) seul(s) constructeur(s) visible(nt) doit(vent) être de portée internal, car on doit permettre uniquement les objets de l'assembly de créer et d'envoyer des évènements.

Cet événement, ainsi que les événements négatifs, doivent être générés lors de la méthode ``CreerFamille`` de la classe ``Famille``. Nous avons plusieurs choix d'implémentations. Un de ceux-ci est de conserver la méthode statique et de demander à notre agrégat de générer les événements de la demande de création. Une fois qu'on récupère l'agrégat dans notre handler, on peut utiliser le dispatcher pour envoyer les événements dans le système. Le problème avec cette méthode et qu'on ne peut se servir des événements que lorsque l'agrégat a été correctement créé. Ainsi, on se prive de la possibilité de valider niveau agrégat de la validation du nom de famille.

Une autre solution que la méthode ``CreerFamille`` renvoie une collection d'événements suite au traitement de la méthode. Dans notre exemple, c'est ce que nous faisons pour bien exposer la réflexion événementielle qu'il doit y avoir à l'origine.

Code à changer côté agrégat :

::

    // Dans les members        
    internal static List<NomFamille> _nomFamilles = new List<NomFamille>();
    
    public static Result CreerFamille(string nom, IEnumerable<Personne> personnes = null)
    {
       NomFamille nomFamille = new NomFamille();
       try
       {
           nomFamille = new NomFamille(nom);
       }
       catch
       {
           return Result.Fail(FamilleNonCreeeCar.NomIncorrect);
       }
       if (_nomFamilles.Any(f => f.Value.Equals(nom, StringComparison.OrdinalIgnoreCase)))
       {
           return Result.Fail(FamilleNonCreeeCar.FamilleDejaExistante);
       }
       _nomFamilles.Add(nomFamille);
       return Result.Ok(nomFamille);
    }
	
Code à changer côté handler :

::    

    public async Task<Result> HandleAsync(AjouterPersonneCommand command, ICommandContext context = null)
    {
       var famille = new Famille(command.NomFamille);
       var resultAjout = famille.AjouterPersonne(command.Prenom, new InfosNaissance(command.LieuNaissance, command.DateNaissance));
       if (resultAjout)
       {
           await famille.PublishDomainEventsAsync();
       }
       return resultAjout;
    }
	
En event-sourcing, les événements sont la source de données et la source de vérité. Ils sont également à la base du flux de l'application. Il est donc nécessaire de capter des événements afin de pouvoir traiter le résultat de la réaction du système, comme mettre éventuellement à jour la base de données, écrire dans un fichier, etc...

Pour ce faire, le comportement est fortement similaire à celui des commands, il faut créer un handler et agir en conséquence. Ici, nous allons en crééer l'handler de l'événement ``FamilleCreeeEvent`` : 

::

    class FamilleCreeeEventHandler : IDomainEventHandler<FamilleCreeeEvent>
    {
        public Task<Result> HandleAsync(FamilleCreeeEvent domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;
    
            Console.ForegroundColor = ConsoleColor.DarkGreen;
    
            Console.WriteLine("La famille " + domainEvent.NomFamille + " a correctement été créée dans le système.");
    
            Console.ForegroundColor = color;
    
            return Result.Ok();
        }
    }


.. note:: Ici, nous n'avons pas de logique métier complexe, le système est sur-dimensionné par rapport aux besoins réel. Dans des cas métier réels complexes, cette séparation et cette granularité est généralement plus un gain qu'un frein.

.. note:: Les informations en cas d'échec (métier ou technique) sont transmises directement à l'appelant lorsqu'il envoie la commande dans le système. Il n'est donc pas nécessaire de créer un process à base d'événement(s) négatif(s).