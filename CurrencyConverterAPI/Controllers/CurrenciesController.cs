using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using ScraperService;
using ScraperService.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<ActionResult<IList<ConvertedRate>>> GetConvertedRates(int sourceCurrency, int amount, DateTime date, [FromQuery] int[] currencies)
        {
            IList<ConvertedRate> convertedRates = await _scraper.GetConvertedRatesAsync(sourceCurrency, amount, date, currencies);
            if (convertedRates.Count == 0)
                return NoContent();

            return Ok(convertedRates);
        }

        [HttpGet("RatesPerPeriod")]
        public async Task<ActionResult<IList<RatesPerDate>>> GetRatesPerPeriod(DateTime startDate, DateTime endDate, [FromQuery] int[] currencies)
        {
            IList<RatesPerDate> ratesPerDates = await _scraper.GetRatesPerDatesAsync(startDate, endDate, currencies);
            if (ratesPerDates.Count == 0)
                return NoContent();

            return Ok(ratesPerDates);
        }

        [HttpGet("DownloadRatesPerPeriodExcel")]
        public async Task<IActionResult> DownloadRatesPerPeriodExcel(DateTime startDate, DateTime endDate, [FromQuery] int[] currencies)
        {
            IList<RatesPerDate> ratesPerDates = await _scraper.GetRatesPerDatesAsync(startDate, endDate, currencies);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("RatesPerDate");
                int currentRow = 1;
                int currentCell = 1;

                #region Header
                worksheet.Cell(currentRow, currentCell++).Value = "Date";
                foreach (var item in ratesPerDates[0].Rates)
                {
                    worksheet.Cell(currentRow, currentCell++).Value = item.CurrencyName;
                }
                #endregion

                #region Body
                foreach (var item in ratesPerDates)
                {
                    currentRow++;
                    currentCell = 1;
                    worksheet.Cell(currentRow, currentCell++).Value = item.Date;
                    foreach (var rate in item.Rates)
                    {
                        worksheet.Cell(currentRow, currentCell++).Value = rate.Amount;
                    }

                }
                #endregion

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();


                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RatesPerDate.xlsx");
                }
            }
        }
    }
}
