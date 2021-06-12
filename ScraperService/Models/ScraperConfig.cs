
namespace ScraperService.Models
{
    public class ScraperConfig
    {
        public string BankOfIsraelURL { get; set; }
        public string[] ConvertQueryStringParams { get; set; }
        public string[] RatesPerPeriodQueryStringParams { get; set; }
        public string CurrenciesSelector { get; set; }
        public string ConvertResultsSelector { get; set; }
        public string RatesPerDateResultsSelector { get; set; }
    }
}
