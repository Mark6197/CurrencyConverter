using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ScraperService.ExtentionMethods;
using ScraperService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScraperService
{
    public class Scraper : IScraper
    {
        private readonly ScraperConfig _scraperConfig;

        //DI for the ScraperConfig- configured in the Startup
        public Scraper(ScraperConfig scraperConfig)
        {
            _scraperConfig = scraperConfig;
        }

        /// <summary>
        /// Get all currenices with scraper
        /// </summary>
        /// <returns>List of all the currencies</returns>
        public async Task<IList<Currency>> GetAllCurrenciesAsync()
        {
            //Get Array with one item only- List of HtmlNodes that are option nodes
            IList<HtmlNode>[] optionNodes = await GetHtmlNodesAsync(_scraperConfig.BankOfIsraelURL, new string[] { _scraperConfig.CurrenciesSelector });

            return ConvertOptionNodesToCurrencies(optionNodes[0]);//Return first item fron this array
        }

        /// <summary>
        /// Get all the convert results with scraper
        /// </summary>
        /// <param name="sourceCurrency">The currency id to convert from</param>
        /// <param name="amount">The amount to convert</param>
        /// <param name="date">The date of the convert, rate convertion will be as per the rate in this date</param>
        /// <param name="currencies">Array of the id's of the currencies to convert to</param>
        /// <returns>List of the convert results</returns>
        public async Task<IList<ConvertedRate>> GetConvertedRatesAsync(int sourceCurrency, int amount, DateTime date, int[] currencies)
        {

            Array.Sort(currencies);
            //Build the url we will scrap from using the parameters and the values fron config file
            string url = _scraperConfig.BankOfIsraelURL + $"&{_scraperConfig.ConvertQueryStringParams[0]}={string.Join(",", currencies)}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[1]}={date.ToString("dd/MM/yyyy")}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[2]}={amount}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[3]}={sourceCurrency}";

            //Get Array that will have one item- List of HtmlNode that are tr nodes
            IList<HtmlNode>[] trNodes = await GetHtmlNodesAsync(url, new string[] { _scraperConfig.ConvertResultsSelector });
            List<ConvertedRate> convertedRates = new();

            //For each tr node create new ConvertedRate instance with the tr's inner td's text
            foreach (var trNode in trNodes[0])
            {
                HtmlNode[] tdNodes = trNode.QuerySelectorAll("td").ToArray();
                bool isConverted = decimal.TryParse(tdNodes[1].InnerText, out decimal result);
                convertedRates.Add(new ConvertedRate(tdNodes[0].InnerText, isConverted ? result : null));
            }

            return convertedRates;
        }

        /// <summary>
        /// Get all the rates per dates results with scraper
        /// </summary>
        /// <param name="startDate">The start date of the period that we are searching</param>
        /// <param name="endDate">The end date of the period that we are searching</param>
        /// <param name="currencies">The currencies that we want to get their rates</param>
        /// <returns></returns>
        public async Task<IList<RatesPerDate>> GetRatesPerDatesAsync(DateTime startDate, DateTime endDate, int[] currencies)
        {
            Array.Sort(currencies);
            //Build the url we will scrap from using the parameters and the values fron config file
            string url = _scraperConfig.BankOfIsraelURL + $"&{_scraperConfig.RatesPerDateQueryStringParams[0]}={string.Join(",", currencies)}" +
                                                     $"&{_scraperConfig.RatesPerDateQueryStringParams[1]}={startDate.ToString("dd/MM/yyyy")}" +
                                                     $"&{_scraperConfig.RatesPerDateQueryStringParams[2]}={endDate.ToString("dd/MM/yyyy")}";

            //Get Array that will have two items- First is List of HtmlNode that are tr nodes, Second is List of HtmlNode that are option nodes
            IList<HtmlNode>[] nodes = await GetHtmlNodesAsync(url, new string[] { _scraperConfig.RatesPerDateResultsSelector, _scraperConfig.CurrenciesSelector });

            //Cobert the List of option nodes to List of Currency objects
            List<Currency> allCurrencies = ConvertOptionNodesToCurrencies(nodes[1]);

            List<RatesPerDate> ratesPerDates = new();

            //Run over each tr node and create new RatesPerDate instance
            foreach (var trNode in nodes[0])
            {
                RatesPerDate ratesPerDate = new RatesPerDate();
                HtmlNode[] tdNodes = trNode.QuerySelectorAll("td").ToArray();
                ratesPerDate.Date = tdNodes[0].InnerText;
                //Run over each td node in the tr node and create new ConvertedRate instance
                for (int i = 1; i < tdNodes.Length; i++)
                {
                    bool isConverted = decimal.TryParse(tdNodes[i].InnerText, out decimal result);
                    string currencyName = allCurrencies.Find(c => c.Id == currencies[i - 1])?.Name;
                    ConvertedRate convertedRate = new ConvertedRate(currencyName, isConverted ? result : null);
                    ratesPerDate.Rates.Add(convertedRate);
                }
                ratesPerDates.Add(ratesPerDate);
            }

            return ratesPerDates;
        }

        /// <summary>
        /// Scrap html nodes from a specific url using the selectors 
        /// </summary>
        /// <param name="url">The url we will scrap from</param>
        /// <param name="selectors">Array of selectors that will lead us to the elements we want to scrap</param>
        /// <returns></returns>
        private async Task<IList<HtmlNode>[]> GetHtmlNodesAsync(string url, string[] selectors)
        {
            HtmlWeb web = new();

            //Create new Array, each item in the Array will be a List of HtmlNodes
            //For each item the selectors array we will create List of HtmlNode that we be an item of the HtmlNodes array, that is the reason their length is the same
            IList<HtmlNode>[] htmlNodes = new IList<HtmlNode>[selectors.Length];
            //Load the HtmlDocument from the url
            HtmlDocument document = await web.LoadFromWebAsync(url);
            //Run over all the selectors
            for (int i = 0; i < selectors.Length; i++)
            {
                //For each selector will extract the HtmlNodes that were chosen by this selector and add the as a List to the Array
                htmlNodes[i] = document.DocumentNode.QuerySelectorAll(selectors[i]).ToList();
            }
            return htmlNodes;
        }

        /// <summary>
        /// Convert HtmlNode List that holds option nodes to a List of Currency objects
        /// </summary>
        /// <param name="optionNodes">List of HtmlNode that holds option nodes</param>
        /// <returns></returns>
        private List<Currency> ConvertOptionNodesToCurrencies(IList<HtmlNode> optionNodes)
        {
            List<Currency> currencies = new();
            //Run over each node and create new currency using the node's value attribute and it's inner text
            foreach (var optionNode in optionNodes)
                currencies.Add(new Currency(optionNode.GetAttributeValue("value", -1), optionNode.InnerText.CleanString()));

            return currencies;
        }

    }
}
