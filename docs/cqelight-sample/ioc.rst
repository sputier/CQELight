Inversion of Control
====================

Nous avons vu dans la précédente étape la création d'un repository. Cette classe nous permet de sauver nos familles dans un fichier texte et de les récupérer au lancement. Il y a cependant un problème avec le code présent : l'appel au repository de familles sous forme de fichier est fait en dur. Cela signifie en substance qu'il est difficile de changer de technologie de persitance au fil de l'application sans réécrire le code, tout en sachant que normalement cette portion de code en production est testée et approuvée. Il faut revoir notre code pour travailler avec des abstractions. Et tant qu'à le revoir, autant respecter les bonnes pratiques avec l'IoC : procéder à l'injection par constructeur (ce qui permet d'afficher sur notre API publique qu'une instance répondant à cette interface est nécessaire pour que la classe fonctionne correctement).

Il est également nécessaire de définir l'interface des méthodes communes qui doivent être implémentées par chaque repository (et bien entendu rajouter dans notre FileFamilleRepository le fait qu'il implémente cette interface) ::
     
    public interface IFamilleRepository 
    {
            Task SauverFamilleAsync(Famille famille);
            Task<Famille> GetFamilleByNomAsync(NomFamille nomFamille);
            Task<IEnumerable<Famille>> GetAllFamillesAsync();
    }
  
Nous allons donc devoir modifier notre handler d'évènement (pour gérer correctement l'évènement FamilleCreee) pour supprimer l'appel qui se fait directement sur ``FileFamilleRepository`` :

::

    class FamilleCreeeEventHandler : IDomainEventHandler<FamilleCreee>, IAutoRegisterType
    {
        private readonly IFamilleRepository _familleRepository;
        
        public FamilleCreeeEventHandler(IFamilleRepository familleRepository)
        {
            _familleRepository = familleRepository ?? throw new ArgumentNullException(nameof(familleRepository));
        }
        
        public async Task<Result> HandleAsync(FamilleCreee domainEvent, IEventContext context = null)
        {
            var color = Console.ForegroundColor;
            try
            {
                await _familleRepository.SauverFamilleAsync(new Data.Models.Famille
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
  
Notre code est retouché pour permettre de travailler avec des abstractions. Mais en l'absence de configuration au niveau du système de CQELight d'un fonctionnement IoC, les handlers ne seront plus appelés, rendant notre système inopérant. Pour ce faire, nous devons, à l'instar du bus in-memory, installer un plugin nous permettant de gérer l'IoC et le configurer.

.. note:: CQELight a fait le choix de n'embarquer aucun module d'IoC ni de développer son propre système afin de laisser le choix aux développeurs de l'outil à utiliser. Le système fonctionne sans IoC tant que la logique des constructeurs sans paramètres est respectée. Si on choisit de l'appliquer à nos handlers si dessus, il faudrait un constructeur sans paramètre qui appelle le constructeur avec paramètre avec l'instance par défaut.

L'un des container les plus utilisés sur le marché est Autofac. CQELight mets à disposition un plugin pour ce dernier. Il suffit d'installer le package correspondant pour commencer à l'utiliser : ``CQELight.IoC.Autofac``. Les spécificités de ce plugin sont décrites dans la page dédié et ne seront pas explorées ici.

Il est nécessaire de retoucher notre ``FileFamilleRepository`` afin d'utiliser la possibilité qu'offre CQELight d'automatiquement enregistrer le type dans le container ::

    class FileFamilleRepository : IFamilleRepository, IAutoRegisterTypeSingleInstance
    {
        private readonly ConcurrentBag<Famille> _familles = new ConcurrentBag<Famille>();
        private string _filePath;
    
        public FileFamilleRepository()
            : this(new FileInfo("./familles.json"))
        {
    
        }
    
        public FileFamilleRepository(FileInfo jsonFile)
        {
            _filePath = jsonFile.FullName;
            var familles = JsonConvert.DeserializeObject<IEnumerable<Famille>>(File.ReadAllText(_filePath));
            if (familles?.Any() == true)
            {
                _familles = new ConcurrentBag<Famille>(familles);
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
	
.. note:: Attention, avec cette méthode, en cas de création d'un nouveau repository, il sera dès lors nécessaire de supprimer l'interface IAutoRegisterTypeSingleInstance du FileSystemRepository pour la mettre sur notre nouvelle implémentation pour que ça soit celle par défaut. D'autre part, la notion de singleton n'a de sens que pour notre repository de fichier car celui-ci utilise une liste mémoire pour gérer le contenu. Le fait d'avoir un singleton oblige également à rendre notre code sécuritaire sur les accès concurrentiels (utilisation d'un ConcurrentBag).

.. note:: Le fait d'utiliser ``IAutoRegisterType`` enregistre le type dans le container par défaut. Ainsi, le container tentera de résoudre chacun des paramètres d'un constructeur, ou utilisera le constructeur sans paramètre s'il n'y arrive pas. Dans notre cas, on a un constructeur qui utilise un fichier par défaut. Cependant, si l'on aurait voulu fournir un autre fichier ou avoir une logique métier du fichier à utiliser par le repository, il aurait été nécessaire de faire un enregistrement manuel dans le container.

On va rajouter au bootstrapper de notre application le fait que le système doit utiliser Autofac comme container IoC ::

    new Bootstrapper()
    .UseInMemoryEventBus()
    .UseInMemoryCommandBus()
    .UseAutofacAsIoC(c => {
        //Les enregistrements manuels se font ici
    })
    .Bootstrapp();
    
Dès que ces opérations sont réalisées, le système fonctionne de façon totalement similaire à précédemment, mais nous offre la possibilité de fournir une autre implémentation pour le ``IFamilleRepository``. A ce titre, comme exercice, vous pouvez essayer de créer un repository qui utilise Entity Framework Core pour stocker les informations dans une base de données et de donner le choix à l'utilisateur au lancement de l'application de quel type de persistance il veut bénéficier.