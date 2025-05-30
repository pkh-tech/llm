using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLM.Models
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string model { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> messages { get; set; }
    }
}
