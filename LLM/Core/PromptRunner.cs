using System;
using System.Collections.Generic;
using System.IO;
using LLM.Models;
using LLM.Utils;

namespace LLM.Core
{
    class PromptRunner
    {
        private List<Message> history;
        public PromptRunner(List<Message> sharedHistory)
        {
            history = sharedHistory;
        }

        public void RunTestPrompt()
        {
            history.Add(new Message("user", File.ReadAllText("TestPrompt/test.llm")));
            string reply = LlmApi.SendToLlm(history);
            string json = JsonUtils.ExtractJson(reply);

            try
            {
                var state = JsonUtils.Deserialize<GameState>(json);
                UiHelper.DisplayGameState(state, json);

                while (true)
                {
                    string next = ConsolePrompt("> ");
                    string choice = SelectionHandler.HandleSelection(state, next);
                    if (choice == null) continue;

                    if (choice.StartsWith("!"))
                    {
                        if (choice.ToLower() == "!test")
                        {
                            RunTestPrompt();
                            return;
                        }

                        Console.WriteLine("[!] Unknown command.");
                        continue;
                    }

                    history.Add(new Message("user", choice));
                    break;
                }
            }
            catch
            {
                Console.WriteLine("\n[⚠ Could not parse JSON – showing raw reply]\n");
                Console.WriteLine(json + "\n");
            }
        }

        private string ConsolePrompt(string text)
        {
            Console.Write(text);
            return Console.ReadLine();
        }
    }
}
