using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.Examples.Console
{
    internal static class ProgramMenus
    {
        #region Static methods

        internal static bool DrawChoiceMenu()
        {
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("Choose your wait to initialize system (enter the number and enter) :");
            System.Console.WriteLine(" 1. Automatic configuration");
            System.Console.WriteLine(" 2. Manual configuration");
            System.Console.ForegroundColor = ConsoleColor.White;
            string choice = string.Empty;
            do
            {
                choice = System.Console.ReadLine();
                if (!choice.In("1", "2"))
                {
                    System.Console.WriteLine($"The choice {choice} is not a valid option, please choose an option from the menu");
                    choice = string.Empty;
                }
            }
            while (string.IsNullOrWhiteSpace(choice));
            return choice == "1";
        }

        internal static void DrawConfigurationMenu()
        {
            System.Console.WriteLine("Not implemented yet ... Press any key to exit.");
            System.Console.Read();
            Environment.Exit(0);
        }

        #endregion

    }
}
