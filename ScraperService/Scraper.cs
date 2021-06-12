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

        public Scraper(ScraperConfig scraperConfig)
        {
            _scraperConfig = scraperConfig;
        }

        public async Task<IList<Currency>> GetAllCurrenciesAsync()
        {
            IList<HtmlNode>[] optionNodes = await GetHtmlNodesAsync(_scraperConfig.BankOfIsraelURL, new string[] { _scraperConfig.CurrenciesSelector });

            return ConvertOptionNodesToCurrencies(optionNodes[0]);
        }

        public async Task<IList<ConvertedRate>> GetConvertedRatesAsync(int sourceCurrency, int amount, DateTime date, int[] currencies)
        {
            Array.Sort(currencies);
            string url = _scraperConfig.BankOfIsraelURL + $"&{_scraperConfig.ConvertQueryStringParams[0]}={string.Join(",", currencies)}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[1]}={date.ToString("dd/MM/yyyy")}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[2]}={amount}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[3]}={sourceCurrency}";

            IList<HtmlNode>[] trNodes = await GetHtmlNodesAsync(url, new string[] { _scraperConfig.ConvertResultsSelector });
            List<ConvertedRate> convertedRates = new();

            foreach (var trNode in trNodes[0])
            {
                HtmlNode[] tdNodes = trNode.QuerySelectorAll("td").ToArray();
                bool isConverted = decimal.TryParse(tdNodes[1].InnerText, out decimal result);
                convertedRates.Add(new ConvertedRate(tdNodes[0].InnerText, isConverted ? result : null));
            }

            return convertedRates;
        }

        public async Task<IList<RatesPerDate>> GetRatesPerDatesAsync(DateTime startDate, DateTime endDate, int[] currencies)
        {
            Array.Sort(currencies);
            string url = _scraperConfig.BankOfIsraelURL + $"&{_scraperConfig.RatesPerPeriodQueryStringParams[0]}={string.Join(",", currencies)}" +
                                                     $"&{_scraperConfig.RatesPerPeriodQueryStringParams[1]}={startDate.ToString("dd/MM/yyyy")}" +
                                                     $"&{_scraperConfig.RatesPerPeriodQueryStringParams[2]}={endDate.ToString("dd/MM/yyyy")}";

            IList<HtmlNode>[] nodes = await GetHtmlNodesAsync(url, new string[] { _scraperConfig.RatesPerDateResultsSelector, _scraperConfig.CurrenciesSelector });

            List<Currency> allCurrencies = ConvertOptionNodesToCurrencies(nodes[1]);

            List<RatesPerDate> ratesPerDates = new();

            foreach (var trNode in nodes[0])
            {
                RatesPerDate ratesPerDate = new RatesPerDate();
                HtmlNode[] tdNodes = trNode.QuerySelectorAll("td").ToArray();
                ratesPerDate.Date = tdNodes[0].InnerText;
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

        private async Task<IList<HtmlNode>[]> GetHtmlNodesAsync(string url, string[] selectors)
        {
            HtmlWeb web = new();

            IList<HtmlNode>[] htmlNodes = new IList<HtmlNode>[selectors.Length];
            HtmlDocument document = await web.LoadFromWebAsync(url);
            for (int i = 0; i < selectors.Length; i++)
            {
                htmlNodes[i] = document.DocumentNode.QuerySelectorAll(selectors[i]).ToList();
            }
            return htmlNodes;
        }

        private List<Currency> ConvertOptionNodesToCurrencies(IList<HtmlNode> optionNodes)
        {
            List<Currency> currencies = new();
            foreach (var optionNode in optionNodes)
                currencies.Add(new Currency(optionNode.GetAttributeValue("value", -1), optionNode.InnerText.RemoveDateFromString()));

            return currencies;
        }

    }
}
