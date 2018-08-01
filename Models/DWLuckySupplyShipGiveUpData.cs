using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWLuckySupplyShipGiveUpDataInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWLuckySupplyShipGiveUpDataModel
    {
        public byte errorCode;
    }
}