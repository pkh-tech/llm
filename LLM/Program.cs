using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Script.Serialization;

namespace LLM
{
    class Program
    {
        static List<Message> history = new List<Message>();
        static JavaScriptSerializer legacyJson = new JavaScriptSerializer();
        static string lastRawJson = "";

        static void Main()
        {
            history.Add(new Message { role = "system", content = File.ReadAllText("System/init.llm") });
            history.Add(new Message { role = "system", content = File.ReadAllText("Game/startArea.llm") });
            history.Add(new Message { role = "user", content = File.ReadAllText("Game/GameStart.llm") });

            Console.WriteLine("Type 'exit' to quit.\n");

            string initReplyJson = SendToLlm(history);
            string initReplyText = ExtractJson(initReplyJson);
            lastRawJson = initReplyText;

            try
            {
                var initState = JsonSerializer.Deserialize<GameState>(initReplyText);
                DisplayGameState(initState);

                while (true)
                {
                    string selection = AskUserInput("> ");
                    string userChoice = HandleSelection(initState, selection);
                    if (userChoice == null) continue;

                    if (userChoice.StartsWith("!"))
                    {
                        if (userChoice.ToLower() == "!test")
                        {
                            RunTestPrompt();
                            continue;
                        }

                        Console.WriteLine("[!] Unknown command.");
                        continue;
                    }

                    history.Add(new Message { role = "user", content = userChoice });
                    break;
                }
            }
            catch
            {
                Console.WriteLine("\n[⚠ Could not parse initial JSON – showing raw reply]\n");
                Console.WriteLine(initReplyText + "\n");
            }

            while (true)
            {
                string input = AskUserInput("> ");
                if (input.ToLower() == "exit") break;

                if (input.ToLower() == "!test")
                {
                    RunTestPrompt();
                    continue;
                }

                history.Add(new Message { role = "user", content = input });

                string replyJson = SendToLlm(history);
                string replyText = ExtractJson(replyJson);
                lastRawJson = replyText;

                try
                {
                    var state = JsonSerializer.Deserialize<GameState>(replyText);
                    DisplayGameState(state);

                    while (true)
                    {
                        string selection = AskUserInput("> ");
                        string userChoice = HandleSelection(state, selection);
                        if (userChoice == null) continue;

                        if (userChoice.StartsWith("!"))
                        {
                            if (userChoice.ToLower() == "!test")
                            {
                                RunTestPrompt();
                                break;
                            }

                            Console.WriteLine("[!] Unknown command.");
                            continue;
                        }

                        history.Add(new Message { role = "user", content = userChoice });
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("\n[⚠ Could not parse JSON – showing raw reply]\n");
                    Console.WriteLine(replyText + "\n");
                }
            }
        }

        static void RunTestPrompt()
        {
            string testPrompt = File.ReadAllText("TestPrompt/test.llm");
            history.Add(new Message { role = "user", content = testPrompt });

            string testReplyJson = SendToLlm(history);
            string testReplyText = ExtractJson(testReplyJson);
            lastRawJson = testReplyText;

            try
            {
                var testState = JsonSerializer.Deserialize<GameState>(testReplyText);
                DisplayGameState(testState);

                while (true)
                {
                    string next = AskUserInput("> ");
                    string nextChoice = HandleSelection(testState, next);
                    if (nextChoice == null) continue;

                    if (nextChoice.StartsWith("!"))
                    {
                        if (nextChoice.ToLower() == "!test")
                        {
                            RunTestPrompt();
                            return;
                        }

                        Console.WriteLine("[!] Unknown command.");
                        continue;
                    }

                    history.Add(new Message { role = "user", content = nextChoice });
                    break;
                }
            }
            catch
            {
                Console.WriteLine("\n[⚠ Could not parse JSON – showing raw reply]\n");
                Console.WriteLine(testReplyText + "\n");
            }
        }

        static string AskUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        static void DisplayGameState(GameState state)
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
            Console.WriteLine(lastRawJson.Length > 300 ? lastRawJson.Substring(0, 300) + "..." : lastRawJson);
            Console.ResetColor();
        }

        static string HandleSelection(GameState state, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.Trim();

            if (input.StartsWith("!"))
                return input;

            if (input.ToLower() == "m")
                return AskUserInput("Your input: ");

            if (int.TryParse(input, out int choice) &&
                choice >= 1 && choice <= state.Options.Count)
                return state.Options[choice - 1];

            Console.WriteLine("[!] Invalid input.");
            return null;
        }

        static string SendToLlm(List<Message> fullHistory)
        {
            var req = new ChatRequest
            {
                model = "local-model",
                messages = fullHistory
            };

            string json = JsonSerializer.Serialize(req);
            var response = PostJson("http://127.0.0.1:1234/v1/chat/completions", json);
            var parsed = JsonSerializer.Deserialize<ChatResponse>(response);

            return parsed.Choices[0].Message.content.Trim();
        }

        static string PostJson(string url, string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.UTF8.GetBytes(json);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
                return reader.ReadToEnd();
        }

        static string ExtractJson(string response)
        {
            int start = response.IndexOf('{');
            int end = response.LastIndexOf('}');
            if (start == -1 || end == -1 || end <= start)
                return response;

            return response.Substring(start, end - start + 1);
        }

        static List<string> WrapLines(string text, int maxWidth)
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

    class ChatRequest
    {
        [JsonPropertyName("model")]
        public string model { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> messages { get; set; }
    }

    class Message
    {
        [JsonPropertyName("role")]
        public string role { get; set; }

        [JsonPropertyName("content")]
        public string content { get; set; }
    }

    class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    class GameState
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("options")]
        public List<string> Options { get; set; }
    }
}
