using Framework.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication9.Interfaces;
using WebApplication9.Models;

namespace WebApplication9.Implementations
{
    public class ExchangeRateFunctionsProvider : IExchangeRateFunctionsProvider
    {
        private readonly StripeRates _stripeRates;
        private readonly IErrorLogger _errors;

        public ExchangeRateFunctionsProvider(IConfiguration configuration, IErrorLogger errors)
        {
            _stripeRates = configuration.GetSection("StripeRates").Get<StripeRates>();
            _errors = errors;
        }

        public async Task<decimal> GetUsdToRsdRateAsync()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-api-key", _stripeRates.ApiKey);

                var result = JsonConvert.DeserializeObject<JToken>((await client.GetStringAsync($"{_stripeRates.Url}usd")).ToString());
                if (result == null)
                    throw new Exception("StripeRate's API result is null.");

                if (result["data"] == null)
                    throw new Exception("JSON Object's 'data' property is null, even though it should be a JSON Array instead.");

                JToken firstObject = JArray.Parse(result["data"].ToString()).FirstOrDefault();
                if (firstObject == null)
                    throw new Exception("JSON Array is empty.");

                var id = firstObject["id"]?.Value<string>();
                var exchangeRate = firstObject["object"]?.Value<string>();

                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception("JSON Object does not have 'id' property.");

                if (id != "usd")
                    throw new Exception("JSON Object's 'id' property is not equal to 'usd' even though the 'id' should be 'usd' because this method is looking for USD -> RSD exchange rate.");

                if (string.IsNullOrWhiteSpace(exchangeRate))
                    throw new Exception("JSON Object does not have inner 'object' property.");

                if (exchangeRate != "exchange_rate")
                    throw new Exception("JSON Object's inner 'object' property is not equal to 'exchange_rate', even though the inner 'object' should be 'exchange_rate' because this method is looking for exchange rate object.");

                JToken rates = firstObject["rates"];
                if (rates == null)
                    throw new Exception("JSON Object's 'rates' property is null, even though it should be another JSON Object instead.");

                var rsdRateObject = rates["rsd"];
                if (rsdRateObject == null)
                    throw new Exception("JSON Object's 'rsd' property is null, even though it should contain a decimal number.");

                return (decimal)rsdRateObject;
            }
            catch(Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "ExchangeRateFunctionsProvider", "GetUsdToRsdRateAsync");
                throw;
            }
        }

        public async Task<decimal> GetRsdToUsdRateAsync()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-api-key", _stripeRates.ApiKey);

                var result = JsonConvert.DeserializeObject<JToken>((await client.GetStringAsync($"{_stripeRates.Url}rsd")).ToString());
                if (result == null)
                    throw new Exception("StripeRate's API result is null.");

                if (result["data"] == null)
                    throw new Exception("JSON Object's 'data' property is null, even though it should be a JSON Array instead.");

                JToken firstObject = JArray.Parse(result["data"].ToString()).FirstOrDefault();
                if (firstObject == null)
                    throw new Exception("JSON Array is empty.");

                var id = firstObject["id"]?.Value<string>();
                var exchangeRate = firstObject["object"]?.Value<string>();

                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception("JSON Object does not have 'id' property.");

                if (id != "rsd")
                    throw new Exception("JSON Object's 'id' property is not equal to 'rsd' even though the 'id' should be 'rsd' because this method is looking for RSD -> USD exchange rate.");

                if (string.IsNullOrWhiteSpace(exchangeRate))
                    throw new Exception("JSON Object does not have inner 'object' property.");

                if (exchangeRate != "exchange_rate")
                    throw new Exception("JSON Object's inner 'object' property is not equal to 'exchange_rate', even though the inner 'object' should be 'exchange_rate' because this method is looking for exchange rate object.");

                JToken rates = firstObject["rates"];
                if (rates == null)
                    throw new Exception("JSON Object's 'rates' property is null, even though it should be another JSON Object instead.");

                var usdRateObject = rates["usd"];
                if (usdRateObject == null)
                    throw new Exception("JSON Object's 'usd' property is null, even though it should contain a decimal number.");

                return (decimal)usdRateObject;
            }
            catch (Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "ExchangeRateFunctionsProvider", "GetRsdToUsdRateAsync");
                throw;
            }
        }

        public async Task<decimal> ConvertUsdToRsdAsync(decimal usdToBeConverted)
        {
            try
            {
                return usdToBeConverted * (await GetUsdToRsdRateAsync());
            }
            catch(Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "ExchangeRateFunctionsProvider", "ConvertUsdToRsdAsync");
                throw;
            }
        }

        public async Task<decimal> ConvertRsdToUsdAsync(decimal rsdToBeConverted)
        {
            try
            {
                return rsdToBeConverted * (await GetRsdToUsdRateAsync());
            }
            catch(Exception e)
            {
                await _errors.SaveErrorAsync(e, "WebApplication9", "ExchangeRateFunctionsProvider", "ConvertRsdToUsdAsync");
                throw;
            }
        }
    }
}
