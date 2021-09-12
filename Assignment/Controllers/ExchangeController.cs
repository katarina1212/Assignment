using Assignment.Services.Interfaces;
using Assignment.Structures.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Assignment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeService _exchangeService;
        public ExchangeController(IExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpPost]
        [Route("GetHistory")]
        public async Task<IActionResult> GetHistoryAsync(ExchangeRateRequest request)
        {
            ExchangeRateResult result = await _exchangeService.GetCurrencyInformationAsync(request);
            return Ok(result);
        }

    }
}
