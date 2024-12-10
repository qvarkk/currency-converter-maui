using CurrencyConverterCore;
using System.Text.Json;

namespace CurrencyConverterMAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            DateSelector.MaximumDate = DateTime.Today;
        }
    }
}
