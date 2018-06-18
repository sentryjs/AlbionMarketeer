using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AlbionMarketeer
{
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
}
