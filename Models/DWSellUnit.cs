﻿using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWSellUnitInputParam
    {
        public string memberID;
        public uint instanceNo;
        public string token;
    }

    [Serializable]
    public class DWSellUnitModel
    {
        public uint instanceNo;
        public long ether;
        public byte errorCode;
    }
}