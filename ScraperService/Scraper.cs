using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ScraperService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            IList<HtmlNode> optionNodes = await GetHtmlNodesAsync(_scraperConfig.CurrenciesURL, _scraperConfig.CurrenciesSelector);

            List<Currency> currencies = new();
            foreach (var optionNode in optionNodes)
                currencies.Add(new Currency(optionNode.GetAttributeValue("value", -1), optionNode.InnerText));

            return currencies;
        }

        public async Task<IList<ConvertedRate>> GetConvertedRatesAsync(int sourceCurrency, int amount, DateTime date, int[] currencies)
        {
            string url = _scraperConfig.ConvertURL + $"&{_scraperConfig.ConvertQueryStringParams[0]}={string.Join(",", currencies)}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[1]}={date.ToShortDateString()}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[2]}={amount}" +
                                                     $"&{_scraperConfig.ConvertQueryStringParams[3]}={sourceCurrency}";

            IList<HtmlNode> trNodes = await GetHtmlNodesAsync(url, _scraperConfig.ConvertResultsSelector);
            List<ConvertedRate> convertedRates = new();

            foreach (var trNode in trNodes)
            {
                HtmlNode[] tdNodes = trNode.QuerySelectorAll("td").ToArray();
                convertedRates.Add(new ConvertedRate(tdNodes[0].InnerText, decimal.Parse(tdNodes[1].InnerText)));
            }

            return convertedRates;
        }

        private async Task<IList<HtmlNode>> GetHtmlNodesAsync(string url, string selector)
        {
            HtmlWeb web = new();
            HtmlDocument document = await web.LoadFromWebAsync(url);
            return document.DocumentNode.QuerySelectorAll(selector).ToList();
        }
    }
}
