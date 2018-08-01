using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWBossDungeonExitInputParam
    {
        public string memberID;
        public short curBossDungeonNo;
        public byte clear;
        public string token;
    }

    [Serializable]
    public class DWBossDungeonExitModel
    {
        public long addGold;
        public long gem;
        public long cashGem;
        public long ether;
        public short lastBossDungeonNo;
        public int bossDungeonTicket;
        public byte errorCode;
    }

}