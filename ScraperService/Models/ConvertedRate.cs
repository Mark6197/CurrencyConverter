using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Models
{
    public class ConvertedRate
    {
        public string CurrencyName { get; set; }
        public decimal Amount { get; set; }

        public ConvertedRate(string currencyName, decimal amount)
        {
            CurrencyName = currencyName;
            Amount = amount;
        }
    }
}
