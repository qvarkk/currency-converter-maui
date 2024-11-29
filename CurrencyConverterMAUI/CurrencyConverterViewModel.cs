using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CurrencyConverterCore;

namespace CurrencyConverterMAUI
{
    internal class CurrencyConverterViewModel : INotifyPropertyChanged
    {

        private readonly CurrencyConverterAPI _api;
        private RatesJson _ratesJson;
        private Dictionary<string, Valute> _rates;
        public ObservableCollection<string> Currencies { get; private set; }

        private string _selectedFromCurrency;
        public string SelectedFromCurrency
        {
            get => _selectedFromCurrency;
            set
            {
                if (_selectedFromCurrency != value)
                {
                    _selectedFromCurrency = value;
                    OnPropertyChanged();
                    ConvertCurrencies();
                }
            }
        }

        private string _selectedToCurrency;
        public string SelectedToCurrency
        {
            get => _selectedToCurrency;
            set
            {
                if (_selectedToCurrency != value)
                {
                    _selectedToCurrency = value;
                    OnPropertyChanged();
                    ConvertCurrencies();
                }
            }
        }

        private string _inputAmount = "1.00";
        public string InputAmount
        {
            get => _inputAmount;
            set
            {
                if (_inputAmount != value)
                {
                    _inputAmount = value;
                    OnPropertyChanged();
                    ConvertCurrencies();
                }
            }
        }

        private string _convertedAmount;
        public string ConvertedAmount
        {
            get => _convertedAmount;
            set
            {
                if (_convertedAmount != value)
                {
                    _convertedAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public CurrencyConverterViewModel()
        {
            _api = new CurrencyConverterAPI();
            Currencies = new ObservableCollection<string>();
            LoadCurrencies();
        }

        private async void LoadCurrencies() { 
            try
            {
                _ratesJson = await _api.CallAPI();
                _rates = _ratesJson.Rates;

                foreach(var rate in _rates)
                {
                    Currencies.Add($"{rate.Value.Name} ({rate.Key})");
                }

                SelectedFromCurrency = Currencies.FirstOrDefault();
                SelectedToCurrency = Currencies.FirstOrDefault();

            } catch(Exception ex)
            {
                Console.WriteLine($"Error loading currencies: {ex.Message}");
            }
        }

        private string GetCurrencyCodeFromCurrenciesEntry(string currencyEntry)
        {
            return currencyEntry.Split('(', ')')[^2];
        }

        private void ConvertCurrencies()
        {
            if (!string.IsNullOrEmpty(SelectedFromCurrency) &&
                !string.IsNullOrEmpty(SelectedToCurrency) &&
                !string.IsNullOrEmpty(InputAmount) &&
                _rates != null)
            {
                string fromCurrencyCode = GetCurrencyCodeFromCurrenciesEntry(SelectedFromCurrency);
                string toCurrencyCode = GetCurrencyCodeFromCurrenciesEntry(SelectedToCurrency);

                if (_rates.ContainsKey(fromCurrencyCode) &&
                    _rates.ContainsKey(toCurrencyCode))
                {
                    Valute fromCurrency = _rates[fromCurrencyCode];
                    Valute toCurrency = _rates[toCurrencyCode];

                    double fromCurrencyRate = fromCurrency.Value / fromCurrency.Nominal;
                    double toCurrencyRate = toCurrency.Value / toCurrency.Nominal;

                    double conversionRate = fromCurrencyRate / toCurrencyRate;
                    double converted = conversionRate * Double.Parse(InputAmount);
                    ConvertedAmount = converted.ToString("F2");
                }
                else
                {
                    ConvertedAmount = string.Empty;
                }
            } 
            else
            {
                ConvertedAmount = string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
