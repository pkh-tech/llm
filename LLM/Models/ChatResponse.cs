using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLM.Models
{
    public class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }
}
