using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace TaxDataFetcher
{
    class DatabaseConnection
    {
        private static MongoClient Client;
        private static IMongoDatabase AxieDatabase;


        public static void SetupConnection(string db = "TaxData")
        {
            var connectionString = KeyGetter.GetDBUrl();

            Client = new MongoClient(connectionString);
            AxieDatabase = Client.GetDatabase(db);
        }

        public static IMongoDatabase GetDb()
        {

            if (Client == null)
            {
                SetupConnection();
            }
            return AxieDatabase;
        }

        public static async Task<int> GetLastTimePriceCheckpoint()
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 2)).FirstOrDefault();
            return Convert.ToInt32(block.block);
        }

        public static async Task SetLastTimePriceCheckpoint(int value)
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 2)).FirstOrDefault();
            block.block = value.ToString();
            await blockCollec.ReplaceOneAsync(b => b.id == 2, block);
        }

        public static async Task<int> GetLastTimeCheckpoint()
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 0)).FirstOrDefault();
            return Convert.ToInt32(block.block);
        }

        public static async Task SetLastTimeCheckpoint(int value)
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 0)).FirstOrDefault();
            block.block = value.ToString();
            await blockCollec.ReplaceOneAsync(b => b.id == 0, block);
        }

        public static async Task<BigInteger> GetLastCheckpoint()
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 1)).FirstOrDefault();
            return BigInteger.Parse(block.block);
        }

        public static async Task SetLastCheckpoint(BigInteger value)
        {
            var blockCollec = GetDb().GetCollection<CheckPoint>("BlockCheckpoint");
            var block = (await blockCollec.FindAsync(b => b.id == 1)).FirstOrDefault();
            block.block = value.ToString();
            await blockCollec.ReplaceOneAsync(b => b.id == 1, block);
        }
    }

    class CheckPoint
    {
        public int id;
        public string block;
    }
}