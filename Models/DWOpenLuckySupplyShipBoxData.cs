using System;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWOpenLuckySupplyShipBoxInputParam
    {
        public string memberID;
        public byte shipIdx;
        public string token;
    }

    [Serializable]
    public class DWOpenLuckySupplyShipBoxModel
    {
        public byte itemIdx;
        public DWItemData itemData;
        public byte errorCode;
    }
}