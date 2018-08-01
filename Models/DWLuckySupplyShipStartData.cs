using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWLuckySupplyShipStartInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWLuckySupplyShipStartModel
    {
        public byte errorCode;
    }
}