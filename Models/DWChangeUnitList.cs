using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWChangeUnitListInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWChangeUnitListModel
    {
        public List<ulong> unitList;
        public int gem;
        public long unitListChangeTime;
        public byte errorCode;
    }
}