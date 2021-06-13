
namespace ScraperService.Models
{
    /// <summary>
    /// Configuration model for the scraper service
    /// </summary>
    public class ScraperConfig
    {
        /// <summary>
        /// The URL that we will scrap data from
        /// </summary>
        public string BankOfIsraelURL { get; set; }
        /// <summary>
        /// Array of the names of the parameters that will be used in the query string for convert actions
        /// Those are the names (NOT THE VALUES) of the parameters that we will need to send together with the URL in order to get convert results
        /// </summary>
        public string[] ConvertQueryStringParams { get; set; }
        /// <summary>
        /// Array of the names of the parameters that will be used in the query string for rates per date actions
        /// </summary>
        public string[] RatesPerDateQueryStringParams { get; set; }
        /// <summary>
        /// The selector that will be used to locate the element that holds the currencies data in the web page
        /// </summary>
        public string CurrenciesSelector { get; set; }
        /// <summary>
        /// The selector that will be used to locate the element that holds the convert results data in the web page
        /// </summary>
        public string ConvertResultsSelector { get; set; }
        /// <summary>
        /// The selector that will be used to locate the element that holds the rates per date results data in the web page
        /// </summary>
        public string RatesPerDateResultsSelector { get; set; }
    }
}
