using Microsoft.AspNetCore.Mvc;
using ScraperService;
using ScraperService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CurrencyConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly IScraper _scraper;

        public CurrenciesController(IScraper scraper)
        {
            _scraper = scraper;
        }

        [HttpGet]
        public async Task<ActionResult<IList<Currency>>> GetCurrencies()
        {
            IList<Currency> currencies = await _scraper.GetAllCurrenciesAsync();

            return Ok(currencies);
        }

        [HttpGet("Convert")]
        public async Task<ActionResult<IList<ConvertedRate>>> ConvertRates(int sourceCurrency, int amount, DateTime date, [FromQuery]int[] currencies)
        {
            IList<ConvertedRate> convertedRates = await _scraper.GetConvertedRatesAsync(sourceCurrency, amount, date, currencies);

            return Ok(convertedRates);
        }
    }
}
