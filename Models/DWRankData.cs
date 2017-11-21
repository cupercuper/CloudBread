using System;
using System.Collections.Generic;
using DW.CommonData;

namespace CloudBread.Models
{
    [Serializable]
    public class DWRankInputParams
    {
        public string memberID;
        public byte rankType;
        public string token;
    }

    [Serializable]
    public class DWRankModel
    {
        public long rankCnt;
        public List<DWRankData> rankList;
        public DWRankData myRankData;
        public byte errorCode;
    }
}