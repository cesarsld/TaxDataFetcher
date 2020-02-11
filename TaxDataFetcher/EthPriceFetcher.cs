using System;
using System.Threading.Tasks;
using TaxDataFetcher;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TaxDataFetcher
{
    public class EthPriceFetcher
    {
        public EthPriceFetcher()
        {
        }
        public static async Task FetchEthPrice()
        {
            var lastCheckpoint = await DatabaseConnection.GetLastTimePriceCheckpoint();
            int now = Convert.ToInt32(((DateTimeOffset)(DateTime.UtcNow)).ToUnixTimeSeconds());
            int gap = (now - lastCheckpoint) / 86400 + 1;
            var newPrices = new List<EthPriceObject>();
            var last = 0;


            var query = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    query = wc.DownloadString("https://min-api.cryptocompare.com/data/v2/histoday?fsym=ETH&tsym=USD&limit=" + gap.ToString());
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            var json = JObject.Parse(query);
            foreach (var obj in json["Data"]["Data"])
            {
                if ((int)obj["time"] > lastCheckpoint)
                {
                    newPrices.Add(new EthPriceObject((int)obj["time"], (float)obj["high"], (float)obj["low"]));
                    last = (int)obj["time"];
                }
            }
            await DatabaseConnection.SetLastTimePriceCheckpoint(last);
            var priceCollec = DatabaseConnection.GetDb().GetCollection<EthPriceObject>("HistoricalEthPrice");
            await priceCollec.InsertManyAsync(newPrices);
        }
    }

    public class EthPriceObject
    {
        public int id;
        public float high;
        public float low;
        public float average;
        public EthPriceObject(int time, float _high, float _low)
        {
            id = time;
            high = _high;
            low = _low;
            average = (high + low) / 2.0f;
        }
    }
}
