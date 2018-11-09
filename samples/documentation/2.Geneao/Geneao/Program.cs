using CQELight;
using CQELight.Dispatcher;
using Geneao.Commands;
using System;
using System.Threading.Tasks;

namespace Geneao
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Bienvenue dans la gestion de votre arbre généalogique");

            new Bootstrapper()
                .UseInMemoryEventBus()
                .UseInMemoryCommandBus()
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
                            Console.WriteLine("Oops, pas encore implémenté ... Désolé");
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

                    Console.WriteLine("Aïïïïïïïe, pas bien, ça a planté ... :(");
                    Console.WriteLine(e.ToString());

                    Console.ForegroundColor = color;
                }
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
            await CoreDispatcher.DispatchCommandAsync(new CreerFamilleCommand(familleName));
        }
    }
}
