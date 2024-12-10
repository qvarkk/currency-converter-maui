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

        private string lastSelectedFromCurrency, lastSelectedToCurrency;
        public ObservableCollection<string> Currencies { get; private set; }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public bool IsAvailable
        {
            get => !_isLoading;
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    LoadCurrencies();
                }
            }
        }

        private string _selectedFromCurrency = "";
        public string SelectedFromCurrency
        {
            get => _selectedFromCurrency;
            set
            {
                if (_selectedFromCurrency != value)
                {
                    _selectedFromCurrency = value;
                    lastSelectedFromCurrency = _selectedFromCurrency;
                    OnPropertyChanged();
                    ConvertCurrencies();
                }
            }
        }

        private string _selectedToCurrency = "";
        public string SelectedToCurrency
        {
            get => _selectedToCurrency;
            set
            {
                if (_selectedToCurrency != value)
                {
                    _selectedToCurrency = value;
                    lastSelectedToCurrency = _selectedToCurrency;
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

        private string _convertedAmount = "1.00";
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

        private async Task LoadCurrencies(int recursionDepth = 0) { 
            try
            {
                IsLoading = true;

                _ratesJson = await _api.CallAPI(_selectedDate);
                _rates = _ratesJson.Rates;

                foreach(var rate in _rates)
                {
                    Currencies.Add($"{rate.Value.Name} ({rate.Key})");
                }

                if (!string.IsNullOrEmpty(lastSelectedFromCurrency))
                    SelectedFromCurrency = Currencies.Where(x => x == lastSelectedFromCurrency).FirstOrDefault("");
                else
                    SelectedFromCurrency = Currencies.FirstOrDefault("");

                if (!string.IsNullOrEmpty(lastSelectedToCurrency))
                    SelectedToCurrency = Currencies.Where(x => x == lastSelectedToCurrency).FirstOrDefault("");
                else
                    SelectedToCurrency = Currencies.FirstOrDefault("");

                SelectedDate = _ratesJson.Date;

                IsLoading = false;

                ConvertCurrencies();

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
            if (Double.TryParse(InputAmount, out double inputAmountDouble) && 
                !string.IsNullOrEmpty(SelectedFromCurrency) &&
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

                    double convertedAmountDouble = CurrencyConverterAPI.ConvertCurrencies(fromCurrency, toCurrency, inputAmountDouble);
                    ConvertedAmount = convertedAmountDouble.ToString("F2");
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
