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
        public int gold;
        public int gem;
        public int enhancedStone;
        public byte errorCode;
    }
}