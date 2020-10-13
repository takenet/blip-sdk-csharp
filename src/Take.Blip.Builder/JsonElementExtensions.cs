using System.Text.Json;

namespace Take.Blip.Builder
{
    public static class JsonElementExtensions
    {
        public static T ToObject<T>(this JsonElement jsonElement)
        {
            var rawText = jsonElement.GetRawText();
            return JsonSerializer.Deserialize<T>(rawText);
        }
    }
}
