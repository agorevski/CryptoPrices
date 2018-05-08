using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPrices.Utilities
{
    internal class API
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coin">The coin that you're interested in</param>
        /// <param name="pair">The value to pair it against (default = USD)</param>
        /// <param name="exchange">The exchange that CryptoCompare supports to query against</param>
        /// <returns></returns>
        internal static Dictionary<string, double> GetPrices(string coin, IEnumerable<string> pairs, string exchange = "")
        {
            var responseBody = string.Empty;

            var coinPart = coin;
            var pairPart = pairs.Any() ? string.Join(",", pairs) : "USD";
            var exchangePart = string.IsNullOrEmpty(exchange) ? string.Empty : "&e=" + exchange;

            var url = @"https://min-api.cryptocompare.com/data/price?fsym=" + coin + "&tsyms=" + pairPart + exchangePart;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }

            var retVal = new Dictionary<string, double>();
            try
            {
                retVal = JsonConvert.DeserializeObject<Dictionary<string, double>>(responseBody);
            }
            catch
            {
                retVal = new Dictionary<string, double>();
            }
            return retVal;
        }
    }
}
