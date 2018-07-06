using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlbionMarketeer
{
    //public struct EnetHeader
    //{
    //    public UInt16 peerId { get; set; }
    //    public byte crcEnabled { get; set; }
    //    public byte commandCount { get; set; }
    //    public uint timestamp { get; set; }
    //    public int challenge { get; set; }

    //    public override string ToString()
    //    {
    //        return "Peer Id : " + peerId + " | CRC Enabled " + crcEnabled + " | Command Count " + commandCount + " | Timestamp " + timestamp + " | Challenge " + challenge;
    //    }
    //}
    //{"Id":153848863,"ItemTypeId":"T2_ARMOR_CLOTH_SET1","ItemGroupTypeId":"T2_ARMOR_CLOTH_SET1","LocationId":2000,"QualityLevel":3,"EnchantmentLevel":0,"UnitPriceSilver":1950000,"Amount":1,"AuctionType":"offer","Expires":"2018-06-06T05:28:11.498938"}
    public struct MarketOrder
    {
        [JsonProperty(PropertyName = "Id")]
        public int ID;

        [JsonProperty(PropertyName = "ItemTypeId")]
        public string ItemID;

        [JsonProperty(PropertyName = "ItemGroupTypeId")]
        public string GroupTypeId;

        [JsonProperty(PropertyName = "LocationId")]
        public int LocationID;

        [JsonProperty(PropertyName = "QualityLevel")]
        public int QualityLevel;

        [JsonProperty(PropertyName = "EnchantmentLevel")]
        public int EnchantmentLevel;

        [JsonProperty(PropertyName = "UnitPriceSilver")]
        public Int64 Price;

        [JsonProperty(PropertyName = "Amount")]
        public int Amount;

        [JsonProperty(PropertyName = "AuctionType")]
        public string AuctionType;

        [JsonProperty(PropertyName = "Expires")]
        public string Expires;
    }

    public partial class GoldPoint
    {
        [JsonProperty(PropertyName = "id")]
        public int Id;

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt;

        [JsonProperty(PropertyName = "updated_at")]
        public string UpdatedAt;

        [JsonProperty(PropertyName = "deleted_at")]
        public string DeletedAt;

        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp;

        [JsonProperty(PropertyName = "price")]
        public int Price;
    }

    public class Orders
    {
        public Orders()
        {
            List = new MarketOrder[0];
        }

        [JsonProperty(PropertyName = "Orders")]
        public MarketOrder[] List;

        public void Clear()
        {
            Array.Clear(this.List, 0, this.List.Length);
        }
    }

    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    //
    //    using AlbionMarketeer;
    //
    //    var apiOrder = ApiOrder.FromJson(jsonString);
    public partial class ApiOrder
    {
        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("quality")]
        public int Quality { get; set; }

        [JsonProperty("sell_price_min")]
        public long SellPriceMin { get; set; }

        [JsonProperty("sell_price_min_date")]
        public string SellPriceMinDate { get; set; }

        [JsonProperty("sell_price_min_volume")]
        public string SellPriceMinVolume { get; set; }

        [JsonProperty("sell_price_max")]
        public long SellPriceMax { get; set; }

        [JsonProperty("sell_price_max_date")]
        public string SellPriceMaxDate { get; set; }

        [JsonProperty("sell_price_max_volume")]
        public string SellPriceMaxVolume { get; set; }

        [JsonProperty("buy_price_min")]
        public long BuyPriceMin { get; set; }

        [JsonProperty("buy_price_min_date")]
        public string BuyPriceMinDate { get; set; }

        [JsonProperty("buy_price_min_volume")]
        public string BuyPriceMinVolume { get; set; }

        [JsonProperty("buy_price_max")]
        public long BuyPriceMax { get; set; }

        [JsonProperty("buy_price_max_date")]
        public string BuyPriceMaxDate { get; set; }

        [JsonProperty("buy_price_max_volume")]
        public string BuyPriceMaxVolume { get; set; }
    }

    public partial class ApiOrder
    {
        public static List<ApiOrder> FromJson(string json) => JsonConvert.DeserializeObject<List<ApiOrder>>(json, AlbionMarketeer.Converter.Settings);
        public void SetName(string name) => Name = name;
    }

    public partial class GoldPoint
    {
        public static List<GoldPoint> FromJson(string json) => JsonConvert.DeserializeObject<List<GoldPoint>>(json, AlbionMarketeer.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<ApiOrder> self) => JsonConvert.SerializeObject(self, AlbionMarketeer.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

