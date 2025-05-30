using System;
using LLM.Models;

namespace LLM.Utils
{
    public static class SelectionHandler
    {
        public static string HandleSelection(GameState state, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.Trim();

            if (input.StartsWith("!"))
                return input;

            if (input.ToLower() == "m")
            {
                Console.Write("Your input: ");
                return Console.ReadLine();
            }

            if (int.TryParse(input, out int choice) &&
                choice >= 1 && choice <= state.Options.Count)
                return state.Options[choice - 1];

            Console.WriteLine("[!] Invalid input.");
            return null;
        }
    }
}
