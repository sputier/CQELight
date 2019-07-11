Accès aux données
=================

Pour notre système, afin d'éviter de tout recommencer à 0 à chaque démarrage, nous allons créer un système de persistance de nos données dans un fichier au format Json. Ce fichier servira également de base pour récupérer les familles existantes lors des demandes métier. Afin de pouvoir répondre à cette problématique, nous allons devoir :

1. Créer les modèles qui seront (dé)sérialisés dans notre fichier Json
2. Créer un repository accessible en lecture et en écriture, mappé sur le fichier Json
3. Ajouter les handlers correspondants aux évènements nécessitant une modification de données

Modèles
^^^^^^^

Un fichier Json pourrait se gérer comme une base NoSQL, nous allons stocker les familles ainsi que les personnes associées sous forme de grappe. Pour ce faire, notre modèle sera une mise à plat du contenu d'une famille ::    

    using CQELight.DAL.Attributes;
    using CQELight.DAL.Common;
    using System.Collections.Generic;
    
    namespace Geneao.Data.Models
    {
        [Table("Familles")]
        public class Famille : IPersistableEntity
        {
            [PrimaryKey]
            public string Nom { get; set; }
            public ICollection Personnes { get; set; }
    
           public object GetKeyValue()
               => Nom;
    
           public bool IsKeySet()
               => !string.IsNullOrWhiteSpace(Nom);
        }
    
        [Table("Personnes")]
        public class Personne : IPersistableEntity
        {
            [Column]
            public string Prenom { get; set; }
            [Column]
            public string LieuNaissance { get; set; }
            [Column]
            public DateTime DateNaissance { get; set; }
            [ForeignKey]
            public Famille Famille { get; set; }
            [Column("NomFamille"), KeyStorageOf(nameof(Famille))]
            public string Famille_Id { get; set; }
    
           public object GetKeyValue()
               => PersonneId;
    
           public bool IsKeySet()
               => PersonneId != Guid.Empty;
        }
    }
 
.. note:: Même si, dans notre cas, les attributs sont inutiles car l'écriture est à plat dans un fichier, cela permet de migrer ultérieurement vers un autre système de persistance de données.

Afin de connaitre la totalité des possiblités offertes par CQELight pour stocker les informations sur une source de persistance, rendez-vous sur :doc:`../cqelight/data-modeling`.

Repository Json
^^^^^^^^^^^^^^^

Pour pouvoir utiliser un fichier comme source de données, nous devons définir une implémentation de repository a utiliser dans nos handlers ::

    class FamilleRepository
    {
        private readonly List<Famille> _familles;
        private string _filePath;
    
        public FileFamilleRepository()
            : this(new FileInfo("./familles.json"))
        {
        }
    
        public FamilleRepository(FileInfo jsonFile)
        {
            _filePath = jsonFile.FullName;
            var familles = JsonConvert.DeserializeObject<IEnumerable<Famille>>(File.ReadAllText(_filePath));
            if (familles?.Any() == true)
            {
                _familles = new List<Famille>(familles);
            }
        }
    
        public Task<IEnumerable<Famille>> GetAllFamillesAsync()
            => Task.FromResult(_familles.AsEnumerable());
    
        public Task<Famille> GetFamilleByNomAsync(NomFamille nomFamille)
            => Task.FromResult(_familles.FirstOrDefault(f => f.Nom.Equals(nomFamille.Value, StringComparison.OrdinalIgnoreCase)));
    
        public Task SauverFamilleAsync(Famille famille)
        {
            _familles.Add(famille);
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_familles));
            return Task.CompletedTask;
        }
    }
	
.. note:: Ici nous avons passé un chemin en dur à notre constructeur par défaut sur un fichier. Il faut que ce fichier existe. Nous devons donc rajouter au début de notre application un test pour voir si le fichier existe, et si non, le créer au format json avec un contenu vide (donc un fichier avec le contenu '[]', sans les apostrophes)

Changement des handlers
^^^^^^^^^^^^^^^^^^^^^^^

Maintenant, il est nécessaire de modifier nos handlers (et notre agrégat famille) pour récupérer les informations depuis le fichier, tout comme il est nécessaire de créer des handlers d'évènements pour mettre à jour le fichier lorsque les opérations ont été réalisées avec succès.

Nous allons commencer par modifier le handler d'évenement de création de famille ::

    class FamilleCreeeEventHandler : IDomainEventHandler<FamilleCreeeEvent>
    {
        public async Task<Result> HandleAsync(FamilleCreeeEvent domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;
            try
            {
                await new FileFamilleRepository().SauverFamilleAsync(new Data.Models.Famille
                {
                    Nom = domainEvent.NomFamille.Value
                }).ConfigureAwait(false);
    
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("La famille " + domainEvent.NomFamille.Value + " a correctement" +
                    " été créée dans le système.");    
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("La famille " + domainEvent.NomFamille.Value + " n'a pas pu être" +
                    " créée dans le système.");
                Console.WriteLine(e.ToString());
                return Result.Fail();
            }
            finally
            {
                Console.ForegroundColor = color;
            }
            return Result.Ok();
        }
    }
	
Notre handler de ``FamilleCreee`` fait maintenant plus que simplement afficher sur la console comme quoi l'opération a réussi ou non au niveau domaine (et donc entièrement en mémoire). Une fois l'opération réussie, la famille est persistée pour les prochaines exécution. Notre domaine reste responsable de la cohérence du système. Cependant, il faut que le domaine soit au courant des informations qui ont été persistées. C'est le rôle du CommandHandler de palier à cette problématique d'infrastructure, il se doit donc de récupérer les informations depuis la persistance et restituer les informations dans le domaine ::

    class CreerFamilleCommandHandler : ICommandHandler<CreerFamilleCommand>
    {
        public async Task<Result> HandleAsync(CreerFamilleCommand command, ICommandContext context = null)
        {
           
               Famille._nomFamilles = (await new FileFamilleRepository().GetAllFamillesAsync()
               .ConfigureAwait(false)).Select(f => new Identity.NomFamille(f.Nom)).ToList();
           
            var result = Famille.CreerFamille(command.Nom);
            if(result && result is Result<NomFamille> resultFamille)
            {
                await CoreDispatcher.PublishEventAsync(new FamilleCreee(resultFamille.Value));
                return Result.Fail();
            }
            return result;
        }
    }
 
Notre agrégat est donc restauré à un état où il connait le contenu des données de la persistance afin de prendre la bonne décision pour l'ensemble du système (parce qu'ici, notre source de vérité est le fichier qui contient l'ensemble des familles). Lors de nos différentes exécutions, on retrouvera l'ensemble de nos familles de cette façon. Grâce au repository, on peut également se permettre d'implémenter une fonction d'affichage de la liste des familles présentes dans le système.

Cependant, il y a un problème majeur avec le code ainsi produit, c'est qu'il ne peut fonctionner qu'avec le repository de fichier. Le jour où, pour des raisons de performances ou de nécessité de stockage, il est nécessaire de stocker les informations en base de données, il sera nécessaire de rechercher tous les appels au FileFamilleRepository pour les remplacer. Et si un retour arrière ou un autre changement est nécessaire, le problème se répètera encore et encore. La solution pour ça consistera à travailler avec des abstractions au niveau code et de laisser CQELight se charger de résoudre les implémentations, comme nous allons le voir dans la partie sur l'IoC.