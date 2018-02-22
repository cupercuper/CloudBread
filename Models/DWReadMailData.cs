﻿using System;

namespace CloudBread.Models
{
    [Serializable]
    public class DWReadMailInputParams
    {
        public string memberID;
        public long index;
        public string token;
    }

    [Serializable]
    public class DWReadMailModel
    {
        public long index;
        public long gold;
        public long gem;
        public long enhancedStone;
        public int bossDungeonTicket;
        public byte errorCode;
    }
}