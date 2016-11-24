using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBot.Models
{
    public class StockQuote
    {
        public class RootObject
        {
            public string Status { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public double LastPrice { get; set; }
            public double Change { get; set; }
            public double ChangePercent { get; set; }
            public string Timestamp { get; set; }
            public int MSDate { get; set; }
            public long MarketCap { get; set; }
            public int Volume { get; set; }
            public double ChangeYTD { get; set; }
            public double ChangePercentYTD { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Open { get; set; }
        }
    }
}