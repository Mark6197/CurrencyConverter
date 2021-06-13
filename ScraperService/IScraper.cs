using ScraperService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScraperService
{
    public interface IScraper
    {
        Task<IList<Currency>> GetAllCurrenciesAsync();
        Task<IList<ConvertedRate>> GetConvertedRatesAsync(int sourceCurrency, int amount, DateTime date, int[] currencies);
        Task<IList<RatesPerDate>> GetRatesPerDatesAsync(DateTime strartDate, DateTime endDate, int[] currencies);
    }
}