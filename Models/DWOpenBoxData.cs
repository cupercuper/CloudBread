using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWOpenBoxDataInputParam
    {
        public string memberID;
        public byte boxType;
        public ulong shopNo;
        public string token;
    }

    [Serializable]
    public class DWOpenBoxDataModel
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
        public List<DWItemData> itemList;
        public byte errorCode;
    }
}