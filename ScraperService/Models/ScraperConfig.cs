
namespace ScraperService.Models
{
    public class ScraperConfig
    {
        public string CurrenciesURL { get; set; }
        public string ConvertURL { get; set; }
        public string[] ConvertQueryStringParams { get; set; }
        public string CurrenciesSelector { get; set; }
        public string ConvertResultsSelector { get; set; }
    }
}
