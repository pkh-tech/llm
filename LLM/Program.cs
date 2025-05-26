using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace LlmChat
{
    class Program
    {
        static void Main(string[] args)
        {
            var history = new List<Message>();
            var serializer = new JavaScriptSerializer();

            Console.WriteLine("Skriv noget (eller 'exit' for at afslutte):");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                    break;

                history.Add(new Message { role = "user", content = input });

                var request = new ChatRequest
                {
                    model = "local-model",
                    messages = history
                };

                string json = serializer.Serialize(request);
                string responseText = PostJson("http://127.0.0.1:1234/v1/chat/completions", json);

                var response = serializer.Deserialize<ChatResponse>(responseText);
                var reply = response.choices[0].message.content.Trim();

                Console.WriteLine("\nLLM: " + reply + "\n");

                history.Add(new Message { role = "assistant", content = reply });
            }
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
    }

    class ChatRequest
    {
        public string model { get; set; }
        public List<Message> messages { get; set; }
    }

    class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    class ChatResponse
    {
        public List<Choice> choices { get; set; }
    }

    class Choice
    {
        public Message message { get; set; }
    }
}
