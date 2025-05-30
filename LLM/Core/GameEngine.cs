using System;
using System.Collections.Generic;
using System.IO;
using LLM.Models;
using LLM.Utils;

namespace LLM.Core
{
    class GameEngine
    {
        private List<Message> history = new List<Message>();
        private string lastRawJson = "";

        public void Run()
        {
            history.Add(new Message("system", File.ReadAllText("Custom/System/init.llm")));
            history.Add(new Message("system", File.ReadAllText("Custom/Game/startArea.llm")));
            history.Add(new Message("user", File.ReadAllText("Custom/Game/GameStart.llm")));

            Console.WriteLine("Type 'exit' to quit.\n");

            string initReply = LlmApi.SendToLlm(history);
            string initJson = JsonUtils.ExtractJson(initReply);
            lastRawJson = initJson;

            try
            {
                var initState = JsonUtils.Deserialize<GameState>(initJson);
                Display(initState);
                HandleUserSelection(initState);
            }
            catch
            {
                Console.WriteLine("\n[⚠ Could not parse initial JSON – showing raw reply]\n");
                Console.WriteLine(initJson + "\n");
            }

            while (true)
            {
                string input = Prompt("> ");
                if (input.ToLower() == "exit") break;

                if (input.ToLower() == "!test")
                {
                    new PromptRunner(history).RunTestPrompt();
                    continue;
                }

                history.Add(new Message("user", input));

                string reply = LlmApi.SendToLlm(history);
                string replyJson = JsonUtils.ExtractJson(reply);
                lastRawJson = replyJson;

                try
                {
                    var state = JsonUtils.Deserialize<GameState>(replyJson);
                    Display(state);
                    HandleUserSelection(state);
                }
                catch
                {
                    Console.WriteLine("\n[⚠ Could not parse JSON – showing raw reply]\n");
                    Console.WriteLine(replyJson + "\n");
                }
            }
        }

        private void HandleUserSelection(GameState state)
        {
            while (true)
            {
                string selection = Prompt("> ");
                string result = SelectionHandler.HandleSelection(state, selection);
                if (result == null) continue;

                if (result.StartsWith("!"))
                {
                    if (result.ToLower() == "!test")
                    {
                        new PromptRunner(history).RunTestPrompt();
                        return;
                    }

                    Console.WriteLine("[!] Unknown command.");
                    continue;
                }

                history.Add(new Message("user", result));
                break;
            }
        }

        private void Display(GameState state)
        {
            UiHelper.DisplayGameState(state, lastRawJson);
        }

        private string Prompt(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
    }
}
