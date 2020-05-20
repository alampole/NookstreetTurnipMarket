using System.Text.Json.Serialization;

namespace NookstreetTurnipMarket.Bot
{
    class ConfigManager
    {
        [JsonPropertyName("token")]
        public string Token { get; private set; }
        [JsonPropertyName("prefix")]
        public string Prefix { get; private set; }
    }
}
