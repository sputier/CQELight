using CQELight;
using CQELight.Dispatcher;
using CQELight.IoC;
using CQELight.EventStore.EFCore.Common;
using Geneao.Commands;
using Geneao.Data;
using Geneao.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using CQELight.Abstractions.DDD;
using Geneao.Domain;

namespace Geneao
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Bienvenue dans la gestion de votre arbre généalogique");
            if (!File.Exists("./familles.json"))
            {
                File.WriteAllText("./familles.json", "[]");
            }
            new Bootstrapper()
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
                .UseAutofacAsIoC(c =>
                {
                })
                .UseEFCoreAsEventStore(
                new CQELight.EventStore.EFCore.EFEventStoreOptions(
                    c => c.UseSqlite("FileName=events.db", opts => opts.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)),
                    archiveBehavior: CQELight.EventStore.SnapshotEventsArchiveBehavior.Delete))
                .Bootstrapp();

            await DisplayMainMenuAsync();
        }

        private static async Task DisplayMainMenuAsync()
        {
            bool isResultOk = true;
            while (true)
            {
                try
                {
                    Console.WriteLine("Choisissez votre commande");
                    Console.WriteLine("1. Lister les familles du logiciel");
                    Console.WriteLine("2. Créer une nouvelle famille");
                    Console.WriteLine("3. Ajouter une personne à une famille");
                    Console.WriteLine("4. Supprimer une famille");

                    Console.WriteLine();
                    var result = Console.ReadKey();
                    Console.WriteLine();
                    switch (result.Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            await ListerFamillesAsync();
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            await CreerFamilleAsync();
                            break;
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            Console.WriteLine("Oops, pas encore implémenté ... Désolé");
                            break;
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            Console.WriteLine("Oops, pas encore implémenté ... Désolé");
                            break;
                        case ConsoleKey.Q:
                            Environment.Exit(0);
                            break;
                        default:
                            isResultOk = false;
                            Console.WriteLine("Choix incorrect, merci de faire un choix dans la liste");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.WriteLine("Aie, pas bien, ça a planté ... :(");
                    Console.WriteLine(e.ToString());

                    Console.ForegroundColor = color;
                }
            }
        }

        private static async Task ListerFamillesAsync()
        {
            using (var scope = DIManager.BeginScope())
            {
                var query = scope.Resolve<IRecupererListeFamille>();
                var familles = await query.ExecuteQueryAsync();
                Console.WriteLine("---- Liste des familles du système ----");
                foreach (var item in familles)
                {
                    Console.WriteLine(item.Nom);
                }
                Console.WriteLine();
            }
        }

        private static async Task CreerFamilleAsync()
        {
            string familleName = string.Empty;
            do
            {
                Console.WriteLine("Choisissez un nom de famille pour la créer");
                familleName = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(familleName));
            var result = await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand(familleName));
            if (!result)
            {
                var color = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.DarkRed;
                string raisonText = string.Empty;
                if (result is Result<FamilleNonCreeeCar> resultErreurFamille)
                {
                    raisonText =
                        resultErreurFamille.Value == FamilleNonCreeeCar.FamilleDejaExistante
                        ? "cette famille existe déjà."
                        : "le nom de la famille est incorrect.";
                }
                if (!string.IsNullOrWhiteSpace(raisonText))
                {
                    Console.WriteLine($"La famille {familleName} n'a pas pu être créée car {raisonText}");
                }

                Console.ForegroundColor = color;
            }
        }
    }
}
