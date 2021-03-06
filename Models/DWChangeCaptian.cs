﻿using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWChangeCaptianInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWChangeCaptianModel
    {
        public byte captianID;
        public long ether;
        public long captianChange;
        public byte errorCode;
    }
}