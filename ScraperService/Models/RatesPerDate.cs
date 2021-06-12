using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperService.Models
{
    public class RatesPerDate
    {
        public string Date { get; set; }
        public IList<ConvertedRate> Rates { get; set; }

        public RatesPerDate()
        {
            Rates = new List<ConvertedRate>();
        }
    }
}
