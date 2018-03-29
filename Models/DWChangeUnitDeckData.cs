using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWChangeUnitDeckInputParam
    {
        public string memberID;
        public uint originInstanceNo;
        public uint changeInstanceNo;
        public byte changeType;
        public string token;
    }

    [Serializable]
    public class DWChangeUnitDeckModel
    {
        public uint originInstanceNo;
        public uint changeInstanceNo;
        public byte errorCode;
    }
}