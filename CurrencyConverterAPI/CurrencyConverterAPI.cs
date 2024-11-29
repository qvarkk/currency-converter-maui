using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverterCore
{
    public class CurrencyConverterAPI
    {
        RestClient client;

        public CurrencyConverterAPI()
        {
            var options = new RestClientOptions("https://www.cbr-xml-daily.ru/");
            client = new RestClient(options);
        }

        public async Task<RatesJson> CallAPI()
        {
            try
            {
                var request = new RestRequest("daily_json.js");
                var response = await client.ExecuteAsync(request);

                if (response == null)
                    throw new InvalidOperationException("Failed to make API call");

                if (!response.IsSuccessful)
                    throw new InvalidOperationException("API call was not successful");

                if (string.IsNullOrWhiteSpace(response.Content))
                    throw new InvalidOperationException("API response is empty");

                if (JsonSerializer.Deserialize<RatesJson>(response.Content) is not RatesJson rates)
                    throw new InvalidOperationException("Couldn't deserialize API response");

                return rates;

            } catch (Exception e)
            {
                Console.WriteLine($"An error occurred while calling the API: {e.Message}");
                return new RatesJson();
            }
        }
    }

    public class RatesJson
    {
        public DateTime Date { get; set; }
        public DateTime PreviousDate { get; set; }
        public string PreviousURL { get; set; } = "";
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Valute")]
        public Dictionary<string, Valute> Rates { get; set; } = new Dictionary<string, Valute>();
    }

    public class Valute 
    {
        public string ID { get; set; } = "";
        public string NumCode { get; set; } = "";   
        public string CharCode { get; set; } = "";
        public int Nominal { get; set; }
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public double Previous { get; set; }
    }
}
