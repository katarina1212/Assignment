using Assignment.Services.Interfaces;
using Assignment.Structures.Config;
using Assignment.Structures.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assignment.Services.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;

        public ExchangeService(IHttpClientFactory httpClientFactory, IOptions<ExchangeApiConfig> config)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = config.Value.Url;
        }

        public async Task<ExchangeRateResult> GetCurrencyInformationAsync(ExchangeRateRequest request)
        {
            ExchangeRateResult result = null;

            if (request.Dates.Count == 1)
            {
                result = await EnsureHistoricalRateAsync(request.Dates.First(), request.BaseCurrency, request.TargetCurrency);
            }
            else if (request.Dates.Count > 1)
            {
                List<DateTime> dates = request.Dates.OrderBy(x => x.Date).ToList();
                result = await EnsureTimeSeriesDataAsync(dates, request.BaseCurrency, request.TargetCurrency);
            }

            return result;
        }

        private async Task<ExchangeRateResult> EnsureHistoricalRateAsync(DateTime date, string baseCurrency, string targetCurrency)
        {
            string dateParam = date.ToString("yyyy-MM-dd");
            string result = await ExecuteHttpGetAsync($"{_baseUrl}/{dateParam}?base={baseCurrency}&symbols={targetCurrency}");
            JObject json = JObject.Parse(result);
            JToken token = json["rates"];
            double singleResult = (double)token.SelectToken(targetCurrency);

            ExchangeRateResult exchangeRateResult = new ExchangeRateResult
            {
                AverageRate = "Maximum rate " + singleResult + " " + dateParam,
                MinimumRate = "Minimum rate " + singleResult + " " + dateParam,
                MaximumRate = "Average rate " + singleResult 
            };

            return exchangeRateResult;
        }

        private async Task<ExchangeRateResult> EnsureTimeSeriesDataAsync(List<DateTime> dates, string baseCurrency, string targetCurrency)
        {
            Dictionary<string, decimal> exchangeHistory = new Dictionary<string, decimal>();

            DateTime firstDate = dates.First();
            DateTime lastDate = dates.Last();
            List<string> formattedDates = dates.Select(x => x.ToString("yyyy-MM-dd")).ToList();

            DateTime secondDate = firstDate.AddYears(1);

            while (firstDate <= lastDate)
            {
                var startDate = firstDate.ToString("yyyy-MM-dd");
                var endDate = secondDate.ToString("yyyy-MM-dd");

                string result = await ExecuteHttpGetAsync($"{_baseUrl}/timeseries?start_date={startDate}&end_date={endDate}&base={baseCurrency}&symbols={targetCurrency}");
                JObject json = JObject.Parse(result);

                IDictionary<string, JToken> rates = (JObject)json["rates"];

                exchangeHistory = exchangeHistory.Concat(rates.Where(x => formattedDates.Contains(x.Key)).ToDictionary(y => y.Key, y => (decimal)y.Value.SelectToken(targetCurrency))).ToDictionary(e => e.Key, e => e.Value);

                firstDate = secondDate.AddDays(1);
                secondDate = firstDate.AddYears(1);

                if (secondDate > lastDate)
                {
                    secondDate = lastDate;
                }
            }

            exchangeHistory = exchangeHistory.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            ExchangeRateResult exchangeRateResult = new ExchangeRateResult
            {
                MaximumRate = "Maximum rate " + (double)exchangeHistory.First().Value + " on " + exchangeHistory.First().Key,
                MinimumRate = "Minimum rate " + (double)exchangeHistory.Last().Value + " on " + exchangeHistory.Last().Key,
                AverageRate = "Average rate "+ (double)exchangeHistory.Average(x => x.Value)
            };

            return exchangeRateResult;
        }

        private async Task<string> ExecuteHttpGetAsync(string url)
        {
            HttpClient client = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("An error occured trying to get data from " + url);
            }
        }
    }
}
