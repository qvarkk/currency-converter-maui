﻿using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverterCore
{
    public class CurrencyConverterAPI
    {
        private RestClient client;

        public CurrencyConverterAPI()
        {
            var options = new RestClientOptions(baseUrl: "https://www.cbr-xml-daily.ru/");
            client = new RestClient(options);
        }

        public async Task<RatesJson> CallAPI(DateTime date)
        {
            string resource = "daily_json.js";

            if (date != DateTime.Today)
                resource = GetArchiveUrl(date);

            try
            {
                var request = new RestRequest(resource);
                var response = await client.ExecuteAsync(request);

                if (response == null)
                    throw new InvalidOperationException("Failed to make API call");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return await CallAPI(date.AddDays(-1));

                if (!response.IsSuccessful)
                    throw new InvalidOperationException("API call was not successful");

                if (string.IsNullOrWhiteSpace(response.Content))
                    throw new InvalidOperationException("API response is empty");

                if (JsonSerializer.Deserialize<RatesJson>(response.Content) is not RatesJson rates)
                    throw new InvalidOperationException("Couldn't deserialize API response");

                // since ruble is not included by default add it manualy
                rates.Rates.Add("RUB", new Valute { CharCode = "RUB", Name = "Российский рубль", Nominal = 1, Value = 1 });

                return rates;

            } catch (Exception e)
            {
                Console.WriteLine($"An error occurred while calling the API: {e.Message}");
                return new RatesJson();
            }
        }

        private string GetArchiveUrl(DateTime date)
        {
            string year = date.Year.ToString();
            string month = date.Month.ToString("00");
            string day = date.Day.ToString("00");

            return $"/archive//{year}//{month}//{day}//daily_json.js";
        }

        public static double ConvertCurrencies(Valute fromCurrency, Valute toCurrency, double inputAmount)
        {
            if (fromCurrency != null && toCurrency != null && inputAmount != 0)
            {
                double fromCurrencyRate = fromCurrency.Value / fromCurrency.Nominal;
                double toCurrencyRate = toCurrency.Value / toCurrency.Nominal;

                double conversionRate = fromCurrencyRate / toCurrencyRate;
                double converted = conversionRate * inputAmount;
                return converted;
            } else
            {
                return 0;
            }
        }
    }
}
