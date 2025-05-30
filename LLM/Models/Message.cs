﻿using System.Text.Json.Serialization;

namespace LLM.Models
{
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public Message(string role, string content)
        {
            Role = role;
            Content = content;
        }

        public Message() { }
    }
}
