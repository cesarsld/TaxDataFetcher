using System;
using MongoDB.Bson;

namespace TaxDataFetcher
{
    public class AuctionSaleData
    {
        public ObjectId _id;
        public int tokenId;
        public float price;
        public string buyer;
        public ulong block;
        public int timestamp;

        public AuctionSaleData(int token, float _price, string _b, ulong _bl, int ts)
        {
            _id = ObjectId.GenerateNewId();
            tokenId = token;
            price = _price;
            buyer = _b;
            block = _bl;
            timestamp = ts;
        }
    }

    public class AuctionCreationData
    {
        public ObjectId _id;
        public int tokenId;
        public string seller;
        public ulong block;
        public int timestamp;

        public AuctionCreationData(int token, string _s, ulong _bl, int ts)
        {
            _id = ObjectId.GenerateNewId();
            tokenId = token;
            seller = _s;
            block = _bl;
            timestamp = ts;
        }
    }

}
