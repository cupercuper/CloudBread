using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBossDungeonEnterInputParam
    {
        public string memberID;
        public byte gemUse;
        public short curBossDungeonNo;
        public string token;
    }

    [Serializable]
    public class DWBossDungeonEnterModel
    {
        public short curBossDungeonNo;
        public byte errorCode;
    }
}