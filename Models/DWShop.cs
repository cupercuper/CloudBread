using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWShopInputParam
    {
        public string memberID;
        public ulong serialNo;
        public string token;
    }

    [Serializable]
    public class DWShopModel
    {
        public double gold;
        public long gem;
        public long cashGem;
        public long ether;
        public long cashEther;
        public long gas;
        public long cashGas;
        public List<SkillItemData> skillItemList;
        public List<BoxData> boxList;
        public long relicBoxCnt;
        public List<LimitShopItemData> limitShopItemDataList;
        public byte errorCode;
    }
}