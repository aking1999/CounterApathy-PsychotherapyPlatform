using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Interfaces
{
    public interface IExchangeRateFunctionsProvider
    {
        Task<decimal> GetUsdToRsdRateAsync();
        Task<decimal> GetRsdToUsdRateAsync();
        Task<decimal> ConvertUsdToRsdAsync(decimal usdToBeConverted);
        Task<decimal> ConvertRsdToUsdAsync(decimal rsdToBeConverted);
    }
}
