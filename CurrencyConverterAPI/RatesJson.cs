using System.Text.Json.Serialization;

namespace CurrencyConverterCore
{
    public class RatesJson
    {
        public DateTime Date { get; set; }
        public DateTime PreviousDate { get; set; }
        public string PreviousURL { get; set; } = "";
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Valute")]
        public Dictionary<string, Valute> Rates { get; set; } = new Dictionary<string, Valute>();
    }
}
