using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudBread;

namespace CloudBread.Models
{
    public class DWGetUserDataInputParams
    {
        public string memberID;
        public string token;
    }

    public class DWGetUserData
    {
        public string MemberID { get; set; }
        public string NickName { get; set; }
        public string RecommenderID { get; set; }
        public short CaptianLevel { get; set; }
        public byte CaptianID { get; set; }
        public short LastWorld { get; set; }
        public short CurWorld { get; set; }
        public short CurStage { get; set; }
        public Dictionary<uint, UnitData> UnitList { get; set; }
        public List<ulong> CanBuyUnitList { get; set; }
        public int Gold { get; set; }
        public int Gem { get; set; }
        public int EnhancedStone { get; set; }
    }
    public class DWGetUserDataModel
    {
        public List<DWGetUserData> UserDataList = new List<DWGetUserData>();
        public byte ErrorCode { get; set; }
    }
}