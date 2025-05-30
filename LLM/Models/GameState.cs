using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLM.Models
{
    public class GameState
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("options")]
        public List<string> Options { get; set; }
    }
}
