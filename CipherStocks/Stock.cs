using System;
using System.Collections.Generic;
using System.Text;

namespace CipherStocks
{
    public class Stock
    {
        public string Symbol { get; set; }
        public string Company { get; set; }
        public double Price { get; set; }
        public double TrailingPE { get; set; }
        public long RegularMarketVolume { get; set; }
        public double RegularMarketChangePercent { get; set; }
        public double TrailingAnnualDividendYield { get; set; }
    }
}
