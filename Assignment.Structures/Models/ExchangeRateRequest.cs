using System;
using System.Collections.Generic;

namespace Assignment.Structures.Models
{
    public class ExchangeRateRequest
    {
        public List<DateTime> Dates { get; set; }
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }

    }
}
