using Assignment.Structures.Models;
using System.Threading.Tasks;

namespace Assignment.Services.Interfaces
{
    public interface IExchangeService
    {
        Task<ExchangeRateResult> GetCurrencyInformationAsync(ExchangeRateRequest request);
    }
}
