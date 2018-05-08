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
            var enableTA = !bool.TryParse(ConfigurationManager.AppSettings["EnableTA"], out bool taResult) ? false : taResult;
            var assessTA = !bool.TryParse(ConfigurationManager.AppSettings["AssessTA"], out bool assessTaResult) ? false : assessTaResult;
            var showForks = !bool.TryParse(ConfigurationManager.AppSettings["EnableForks"], out bool forkResult) ? false : forkResult;

            var best = ConfigurationManager.AppSettings["Best"];
            var great = ConfigurationManager.AppSettings["Great"];
            var good  = ConfigurationManager.AppSettings["Good"];
            var monitor = ConfigurationManager.AppSettings["Monitor"];
            var bad = ConfigurationManager.AppSettings["Bad"];

            if (enableTA)
            {
                var tradeSignals = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("TradeSignal")).Select(key => ConfigurationManager.AppSettings[key]).Select(values =>
                {
                    var data = values.Split(',');
                    var coin = data[0].Trim();
                    var pair = data[1].Trim();
                    var exchange = data[2].Trim();
                    var entry = double.Parse(data[3].Trim());
                    var target1 = double.Parse(data[4].Trim());
                    var target2 = double.Parse(data[5].Trim());
                    var stopLoss = double.Parse(data[6].Trim());
                    return new Tuple<string, string, string, double, double, double, double>(coin, pair, exchange, entry, target1, target2, stopLoss);
                });

                if (!tradeSignals.Any())
                {
                    throw new ArgumentException("No TradeSignals found in App Settings!");
                }

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

                        var log = $"{coin}/{pair} ({exchange}) - Price: {curPrice.ToString("#.######")} || E: {entry.ToString("#.######")} | T1: {t1.ToString("#.######")} | T2: {t2.ToString("#.######")} | Stop: {stop.ToString("#.######")} || P/L: {profitLoss.ToString("###.#")}% ";
                        if (assessTaResult)
                        {
                            log = log + Environment.NewLine;
                            if (curPrice > t2) { log += best; }
                            else if (curPrice > t1) { log += great; }
                            else if (curPrice > entry) { log += good; }
                            else if (curPrice > stop) { log += monitor; }
                            else if (curPrice <= stop) { log += bad; }
                        }
                        logBuilder.Add(log);

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


            if (showForks)
            {
                while (true)
                {
                    var logBuilder = new ConcurrentBag<string>();

                    var bitcoinForks = ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("Fork")).Select(key => ConfigurationManager.AppSettings[key]).Select(values =>
                    {
                        var data = values.Split(',');
                        var coin = data[0].Trim();
                        var pair = "USD";
                        var multiplier = int.Parse(data[1].Trim());
                        return new Tuple<string, string, int>(coin, pair, multiplier);
                    });

                    if (!bitcoinForks.Any())
                    {
                        throw new ArgumentException("No Bitcoin Forks found in AppSettings!");
                    }


                    Parallel.ForEach(bitcoinForks, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, fork =>
                    {
                        var coin = fork.Item1;
                        var pair = fork.Item2;
                        var multiplier = fork.Item3;

                        var getPrice = API.GetPrices(coin, new[] { pair });
                        if (!getPrice.ContainsKey(pair)) { return; }

                        var curPrice = getPrice[pair];
                        var withMult = curPrice * multiplier;
                        logBuilder.Add($"{coin}/{pair} ({multiplier}x) || Cur: ${curPrice.ToString("####.######")} | True: ${withMult.ToString("####.######")}");
                    });

                    foreach (var log in logBuilder)
                    {
                        Console.WriteLine(log);
                    }
                    logBuilder = new ConcurrentBag<string>();
                    Thread.Sleep(TimeSpan.FromSeconds(refreshRateInSeconds));
                }
            }
        }
    }
}
