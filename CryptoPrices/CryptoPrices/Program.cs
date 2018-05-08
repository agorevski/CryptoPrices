using CryptoPrices.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

using System.Linq;
using System.Net;

namespace CryptoPrices
{
    class Program
    {
        static void Main(string[] args)
        {
            var pairs = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("Pair")).Select(key => ConfigurationManager.AppSettings[key]).Select(values =>
            {
                var data = values.Split(',');
                var coin = data[0];
                var pair = data[1];
                var exchange = data[2];
                var entry = double.Parse(data[3]);
                var target1 = double.Parse(data[4]);
                var target2 = double.Parse(data[5]);
                var stopLoss = double.Parse(data[6]);
                return new Tuple<string, string, string, double, double, double, double>(coin, pair, exchange, entry, target1, target2, stopLoss);
            });
        }
    }
}