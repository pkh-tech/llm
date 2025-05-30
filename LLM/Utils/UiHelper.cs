using System;
using System.Collections.Generic;
using System.Text;
using LLM.Models;

namespace LLM.Utils
{
    public static class UiHelper
    {
        public static void DisplayGameState(GameState state, string rawJson)
        {
            Console.WriteLine("\n--------------------------------------------------");

            var lines = WrapLines(state.Description, 60);
            foreach (var line in lines)
                Console.WriteLine(line);
            for (int i = lines.Count; i < 10; i++)
                Console.WriteLine();

            Console.WriteLine("--------------------------------------------------");

            int perLine = (int)Math.Ceiling(state.Options.Count / 2.0);
            for (int line = 0; line < 2; line++)
            {
                var optionLine = new StringBuilder();
                for (int i = 0; i < perLine; i++)
                {
                    int index = line * perLine + i;
                    if (index >= state.Options.Count) break;
                    optionLine.Append($"[{index + 1}] {state.Options[index]}    ");
                }
                Console.WriteLine(optionLine.ToString().TrimEnd());
            }

            Console.WriteLine("[M] Manual input");
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[DEBUG] Raw JSON:");
            Console.WriteLine(rawJson.Length > 300 ? rawJson.Substring(0, 300) + "..." : rawJson);
            Console.ResetColor();
        }

        private static List<string> WrapLines(string text, int maxWidth)
        {
            var result = new List<string>();
            var words = text.Split(' ');
            var line = new StringBuilder();

            foreach (var word in words)
            {
                if (line.Length + word.Length + 1 > maxWidth)
                {
                    result.Add(line.ToString());
                    line.Clear();
                }

                if (line.Length > 0)
                    line.Append(" ");

                line.Append(word);
            }

            if (line.Length > 0)
                result.Add(line.ToString());

            return result;
        }
    }
}
