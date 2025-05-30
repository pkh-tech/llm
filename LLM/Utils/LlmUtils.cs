using System.Collections.Generic;
using System.Text.Json;
using LLM.Models;

namespace LLM.Utils
{
    public static class LlmApi
    {
        public static string SendToLlm(List<Message> fullHistory)
        {
            var req = new ChatRequest
            {
                model = "local-model",
                messages = fullHistory
            };

            string json = JsonSerializer.Serialize(req);
            string response = HttpUtils.PostJson("http://127.0.0.1:1234/v1/chat/completions", json);

            var parsed = JsonSerializer.Deserialize<ChatResponse>(response);
            return parsed.Choices[0].Message.Content.Trim();
        }
    }
}
