using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBot.Models
{

    public class exchangeRate
    {
        public class RootObject
        {
            public string @base { get; set; }
            public string date { get; set; }
            public Dictionary<string, int> rates { get; set; }
        }

    }


}