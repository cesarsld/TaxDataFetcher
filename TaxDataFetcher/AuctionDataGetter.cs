using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using TaxDataFetcher;

namespace TaxDataFetcher
{
    public class AuctionDataGetter
    {
        #region ABI & contract declaration
        private static string AxieCoreContractAddress = "0xF4985070Ce32b6B1994329DF787D1aCc9a2dd9e2";
        #endregion

        public AuctionDataGetter()
        {
        }
        public static async Task FetchSalesData()
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/146c5ff4a83a4a62b8eb4bbc93e07974");
            //get contracts
            var auctionContract = web3.Eth.GetContract(KeyGetter.GetABI("auctionABI"), AxieCoreContractAddress);

            //get events
            var auctionSuccesfulEvent = auctionContract.GetEvent("AuctionSuccessful");
            var auctionCreatedEvent = auctionContract.GetEvent("AuctionCreated");

            BigInteger first = 5316433; // This block is the start of the Axie Marketplace contract
            BigInteger last = (await GetLastBlockCheckpoint(web3)).BlockNumber.Value;
            BigInteger current = await DatabaseConnection.GetLastCheckpoint();

            if (current == 1)
                current = first;
            var auctionList = new List<AuctionSaleData>();
            var auctionCreateList = new List<AuctionCreationData>();

            while (current < last)
            {
                var latest = current + 20000;
                if (latest > last)
                    latest = last;
                Console.WriteLine($"Scanning blocks {current}-{latest}");
                
                var auctionFilterAll = auctionSuccesfulEvent.CreateFilterInput(new BlockParameter(new HexBigInteger(current)), new BlockParameter(new HexBigInteger(latest)));
                var auctionLogs = await auctionSuccesfulEvent.GetAllChanges<AuctionSuccessfulEvent>(auctionFilterAll);

                var auctionCreationFilterAll = auctionCreatedEvent.CreateFilterInput(new BlockParameter(new HexBigInteger(current)), new BlockParameter(new HexBigInteger(latest)));
                var creationLogs = await auctionCreatedEvent.GetAllChanges<AuctionCreatedEvent>(auctionCreationFilterAll);

                foreach (var log in auctionLogs)
                {
                    float price = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.totalPrice).ToString());
                    int token = Convert.ToInt32(log.Event.tokenId.ToString());
                    auctionList.Add(new AuctionSaleData(token, price, log.Event.winner, Convert.ToUInt64(log.Log.BlockNumber.Value.ToString()),
                        await GetBlockTimeStamp(log.Log.BlockNumber.Value, web3)));
                }
                foreach (var log in creationLogs)
                {
                    int token = Convert.ToInt32(log.Event.tokenId.ToString());
                    auctionCreateList.Add(new AuctionCreationData(token, log.Event.seller, Convert.ToUInt64(log.Log.BlockNumber.Value.ToString()),
                        await GetBlockTimeStamp(log.Log.BlockNumber.Value, web3)));
                }
                var auctionCollec = DatabaseConnection.GetDb().GetCollection<AuctionSaleData>("AuctionSales");
                var auctionCreateCollec = DatabaseConnection.GetDb().GetCollection<AuctionCreationData>("AuctionCreations");
                if (auctionList.Count > 0)
                    await auctionCollec.InsertManyAsync(auctionList);
                if (auctionCreateList.Count > 0)
                    await auctionCreateCollec.InsertManyAsync(auctionCreateList);
                auctionList.Clear();
                auctionCreateList.Clear();
                current = latest;
            }
            await DatabaseConnection.SetLastCheckpoint(last);
            Console.WriteLine("Done!");

        }

        private static async Task<BlockParameter> GetLastBlockCheckpoint(Web3 web3)
        {
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumber = lastBlock.Value - 10;
            return new BlockParameter(new HexBigInteger(blockNumber));
        }


        private static async Task<int> GetBlockTimeStamp(BigInteger number, Web3 web3)
        {
            var blockParam = new BlockParameter(new HexBigInteger(number));
            var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockParam);
            return Convert.ToInt32(block.Timestamp.Value.ToString());
        }

    }



    [Event("AuctionSuccessful")]
    public class AuctionSuccessfulEvent : IEventDTO
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public string nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public BigInteger tokenId { get; set; }

        [Parameter("uint256", "_totalPrice", 3)]
        public BigInteger totalPrice { get; set; }

        [Parameter("address", "_winner", 4)]
        public string winner { get; set; }
    }

    [Event("AuctionCreated")]
    public class AuctionCreatedEvent : IEventDTO
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public string nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public BigInteger tokenId { get; set; }

        [Parameter("uint256", "_startingPrice", 3)]
        public BigInteger startingPrice { get; set; }

        [Parameter("uint256", "_endingPrice", 4)]
        public BigInteger endingPrice { get; set; }

        [Parameter("uint256", "_duration", 5)]
        public BigInteger duration { get; set; }

        [Parameter("address", "_seller", 6)]
        public string seller { get; set; }

    }


}
