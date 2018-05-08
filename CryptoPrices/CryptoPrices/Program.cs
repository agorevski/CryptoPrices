using CryptoPrices.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoPrices
{
    class Program
    {
        static void Main(string[] args)
        {
            var refreshRateInSeconds = !int.TryParse(ConfigurationManager.AppSettings["RefreshRateInSeconds"], out int result) || result < 30 ? 30 : result;

            var tradeSignals = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("TradeSignal")).Select(key => ConfigurationManager.AppSettings[key]).Select(values =>
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

            var logBuilder = new ConcurrentBag<string>();
            while (true)
            {
                Parallel.ForEach(tradeSignals, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, tradeSignal =>
                {
                    var coin = tradeSignal.Item1;
                    var pair = tradeSignal.Item2;
                    var exchange = tradeSignal.Item3;
                    var entry = tradeSignal.Item4;
                    var t1 = tradeSignal.Item5;
                    var t2 = tradeSignal.Item6;
                    var stop = tradeSignal.Item7;

                    var coinPrice = API.GetPrices(coin, new[] { pair }, exchange);
                    if (!coinPrice.ContainsKey(pair)) { return; }
                    var curPrice = coinPrice[pair];
                    var profitLoss = (1 - (entry / curPrice)) * 100;

                    logBuilder.Add($"{coin}/{pair} ({exchange}) - Price: {curPrice.ToString("#.######")} || E: {entry.ToString("#.######")} | T1: {t1.ToString("#.######")} | T2: {t2.ToString("#.######")} | Stop: {stop.ToString("#.######")} || P/L: {profitLoss.ToString("###.#")}% ");
                });
                Console.Clear();
                foreach (var log in logBuilder.OrderByDescending(e => e))
                {
                    Console.WriteLine(log);
                }
                logBuilder = new ConcurrentBag<string>();
                Thread.Sleep(TimeSpan.FromSeconds(refreshRateInSeconds));
            }

        }
    }
}
