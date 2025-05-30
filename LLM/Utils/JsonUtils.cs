using System;
using System.Text.Json;

namespace LLM.Utils
{
    public static class JsonUtils
    {
        public static string ExtractJson(string response)
        {
            int start = response.IndexOf('{');
            int end = response.LastIndexOf('}');
            if (start == -1 || end == -1 || end <= start)
                return response;

            return response.Substring(start, end - start + 1);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
