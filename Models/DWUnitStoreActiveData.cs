using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    public class DWUnitStoreActiveInputParam
    {
        public string memberID;
        public string token;
    }

    public class DWUnitStoreActiveModel
    {
        public byte unitStore;
        public int gem;
        public byte errorCode;
    }
}