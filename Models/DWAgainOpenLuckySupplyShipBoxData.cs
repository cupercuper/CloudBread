using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWAgainOpenLuckySupplyShipBoxInputParam
    {
        public string memberID;
        public byte shipIdx;
        public string token;
    }

    [Serializable]
    public class DWAgainOpenLuckySupplyShipBoxModel
    {
        public byte itemIdx;
        public DWItemData itemData;
        public byte errorCode;
    }
}