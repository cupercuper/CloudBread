using System;
using System.Collections.Generic;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUnitSlotUpgradeInputParam
    {
        public string memberID;
        public string token;
    }

    [Serializable]
    public class DWUnitSlotUpgradeModel
    {
        public byte unitSlotIdx;
        public int gem;
        public byte errorCode;
    }


}