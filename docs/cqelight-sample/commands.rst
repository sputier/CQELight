Commands
========
Notre domaine étant modélisé, il est actuellement nécessaire de faire un appel direct, avec un couplage fort, à l'agrégat Famille pour récupérer une Famille (méthode ``CreerFamille`` ou constructeur). Afin d'éviter ce couplage fort avec notre domaine et permettre au domaine d'évoluer sans impacter les appelants, nous allons définir une commande. Pour rappel, une commande est un simple DTO chargé de véhiculer les informations suffisantes pour s'exécuter et elle doit également vérifier l'intégrité des données qu'elle transporte (pas de vérification métier à son niveau, juste de la vérification technique).

Notre première commande porterait donc la notion de création d'une famille ::

    using CQELight.Abstractions.CQS.Interfaces;
    using System;
    
    namespace Geneao.Commands
    {
        public sealed class CreerFamilleCommand : ICommand
        {
            public string Nom { get; private set; }
    
            private CreerFamilleCommand() { }
    
            public CreerFamilleCommand(string nom)
            {
                if (string.IsNullOrWhiteSpace(nom))
                {
                    throw new ArgumentException("CreerFamilleCommand.ctor() : Un nom doit être fourni.", nameof(nom));
                } 
                Nom = nom;
            }
        }
    }
         
Plusieurs élements sont importants dans cette portion de code :

- Tout d'abord, les properties sont uniquement affectées depuis le constructeur, ce qui permet d'assurer leur validité. Il est essentiel de faire cette vérification, car on considère que dès lors qu'une commande arrive dans un handler, elle est valide (c'est à dire que le handler n'a pas à vérifier le contenu des membres ni leur cohérence.) Attention, ce n'est pas à la commande, simple DTO, de faire des vérifications métier (comme ici, par exemple, la taille du nom de famille).
- Il y a un constructeur, sans paramètres, privé. En effet, il n'est pas improbable que votre commande passe par des passerelles de communication (bus, appel API, ...), et devra donc être sérialisée/déserialisée. Il faut donc un point d'entrée pour le moteur de sérialisation, et les setters inaccessibles pour une utilise normale mais utilisable par le moteur.
- La classe implémente l'interface ``CQELight.Abstractions.CQS.Interfaces.ICommand``, qui ne contient rien du tout. Elle est uniquement là pour un typage fort. Attention lors de l'import de l'using de ne pas prendre celui de ``System.Windows.Input``.
- Le nom de la classe d'une commande utiliser toujours un verbe à l'infinitif, car on "ordonne" au système de faire une action. Grâce à ce nommage, on peut se passer du suffixe *Command*, selon les affinités des équipes, ou les normes de code. Dans l'exemple nous gardons le suffixe pour que vous puissiez facilement retrouver vos objets dans le projet.
- Etant donné qu'une commande est l'entrée dans le sytème, il faut verrouiller les accès non autorisés avec des données farfelues, du coup, on définie la classe comme sealed, pour éviter des héritages inattendus depuis l'extérieur du système.

On peut dès lors propager cette commande dans le système une fois qu'elle est créée. Ceci se fait très simplement ::

    await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand("NomTest")).ConfigureAwait(false);
         
.. note:: Dans cet exemple, nous utilisons l'API statique du dispatcher, qui est effectivement plus simple d'accès (pas d'instanciation). Il est recommandé, dans la grande majorité des cas, d'utiliser l'API d'instance (avec un ``IDispatcher`` injecté en paramètre de constructeur, ou en utilisant directement la classe ``BaseDispatcher`` ou toute autre implémentation que vous en auriez faite) afin d'éviter les accès concurrentiels et les problèmes associés (lock, performances, ...)

De cette façon, la commande de demande de création d'une personne avec les informations renseignées va être propagée dans le système. Par contre, il n'y a aucun point d'arrivée pour traiter cette commande. Ici, il y a deux possibilités : soit notre Aggregate peut réagir directement à la commande (en implémentant l'interface ``ICommandHandler``), soit on crée un handler spécifique pour traiter cette commande. La création du handler est fortement recommandée pour la quasi totalité des cas, car passer par cette étape intermédiaire permet de résoudre les problèmes infrastructuraux associés au traitement de la commande (chargement d'informations depuis une source de données, récupération d'un aggregat complexe, ...) ::

     using CQELight.Abstractions.CQS.Interfaces;
     using Geneao.Commands;
     using Geneao.Domain;
     using System.Threading.Tasks;
     
     namespace Geneao.Handlers.Commands
     {
         public class CreerFamilleCommandHandler : ICommandHandler<CreerFamilleCommand>
         {
             private static List<Famille> _familles = new List<Famille>();
             public Task<Result> HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
             {
                var result = Famille.CreerFamille(command.Nom);
                if(result && result is Result<NomFamille> resultFamille)
                {
                    await CoreDispatcher.PublishEventAsync(new FamilleCreee(resultFamille.Value));
                }
                return result;
             }
         }
     }
        
L'handler de la command est une classe qui implémente l'interface ``ICommandHandler``, pour la commande qu'on veut gérer. Une seule méthode est à définir, ``HandleAsync``. Ici, le comportement est anecdotique et se veut être pour l'exemple, sans réel impact sur le système. On ajoutera dans un prochain temps la récupération des familles depuis une source de données pour permettre au domaine de prendre la meilleure décision.

Une autre particularité est qu'un handler de command renvoie un objet de type ``Result``. Cet objet n'est **PAS** là pour remplacer la notion évenementielle, mais pour avertir l'appelant de l'échec ou du succès de son appel. Dans notre cas, l'échec contient une notion métier qui peut être utile au code qui a envoyé la commande, mais cette notion de résultat est également utilisée par le système pour déterminer de la suite des actions à entreprendre.

.. note:: Il est possible de procéder différement ici et de ne pas retourner le ``Result`` obtenu par l'appel métier mais de retourner un ``Result`` épuré de ces notions. De la même façon, il est possible gérer des événements positifs comme des événements négatifs, mais nous aurions alors une problématique de nombre (pour chaque action/command, au minimum deux événéments : un positif et un négatif) et de pertinence (dans un système event-sourcé, les évenements négatifs n'ont aucun sens).

Il y a bien sûr, plusieurs commands pour le domaine. A titre d'exercice, et avant de consulter la solution, vous pouvez vous entrainer et créer les commandes (ainsi que les handlers) pour les actions : *AjouterPersonne*, *SupprimerFamille*.