using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWUseActiveItemInputParam
    {
        public string memberID;
        public ulong serialNo;
        public string token;
    }

    [Serializable]
    public class DWUseActiveItemModel
    {
        public long gem;
        public long cashGem;
        public List<ActiveItemData> activeItemList;
        public byte errorCode;
    }
}