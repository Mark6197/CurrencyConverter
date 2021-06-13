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

        //DI for the IScraper- configured in the Startup
        public CurrenciesController(IScraper scraper)
        {
            _scraper = scraper;
        }

        /// <summary>
        /// Get list of all the currencies
        /// </summary>
        /// <returns>List of all the currencies</returns>
        /// <response code="200">Returns the list of currencies</response>
        [HttpGet]
        public async Task<ActionResult<IList<Currency>>> GetCurrencies()
        {
            IList<Currency> currencies = await _scraper.GetAllCurrenciesAsync();

            return Ok(currencies);
        }

        /// <summary>
        /// Get list of all the convert results
        /// </summary>
        /// <returns>List of all the convert results</returns>
        /// <response code="200">Returns the list of the convert results</response>
        /// <response code="204">If the are no convert results</response>

        [HttpGet("Convert")]
        public async Task<ActionResult<IList<ConvertedRate>>> GetConvertedRates(int sourceCurrency, int amount, DateTime date, [FromQuery] int[] currencies)
        {
            IList<ConvertedRate> convertedRates = await _scraper.GetConvertedRatesAsync(sourceCurrency, amount, date, currencies);
            if (convertedRates.Count == 0)
                return NoContent();

            return Ok(convertedRates);
        }

        /// <summary>
        /// Get list of all the rates per period results
        /// </summary>
        /// <returns>List of all the rates per period results</returns>
        /// <response code="200">Returns the list of the rates per period results</response>
        /// <response code="204">If the are no rates per period results</response>
        [HttpGet("RatesPerPeriod")]
        public async Task<ActionResult<IList<RatesPerDate>>> GetRatesPerPeriod(DateTime startDate, DateTime endDate, [FromQuery] int[] currencies)
        {
            IList<RatesPerDate> ratesPerDates = await _scraper.GetRatesPerDatesAsync(startDate, endDate, currencies);
            if (ratesPerDates.Count == 0)
                return NoContent();

            return Ok(ratesPerDates);
        }

        /// <summary>
        /// Get (download) list of all the rates per period results in excel file
        /// </summary>
        /// <returns>List of all the rates per period results</returns>
        /// <response code="200">Returns the list of the rates per period results in excel file</response>
        [HttpGet("DownloadRatesPerPeriodExcel")]
        public async Task<IActionResult> DownloadRatesPerPeriodExcel(DateTime startDate, DateTime endDate, [FromQuery] int[] currencies)
        {
            //Get the results from the scrapper
            IList<RatesPerDate> ratesPerDates = await _scraper.GetRatesPerDatesAsync(startDate, endDate, currencies);

            //Create new Xl work book
            using (var workbook = new XLWorkbook())
            {
                //Add worksheet to the workbook
                var worksheet = workbook.Worksheets.Add("RatesPerDate");
                //Define field current row and current cell
                int currentRow = 1;
                int currentCell = 1;

                //Create the headers (First row) in the sheet
                #region Header
                worksheet.Cell(currentRow, currentCell++).Value = "Date";
                foreach (var item in ratesPerDates[0].Rates)
                {
                    worksheet.Cell(currentRow, currentCell++).Value = item.CurrencyName;
                }
                #endregion

                //Create the body (All other rows) in the sheet
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

                //Return FileContentResult that will contain the stream, the content type that will be in the response headers and name of the file
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
