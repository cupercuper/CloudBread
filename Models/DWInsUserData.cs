using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWInsUserDataInputParams
    {
        public string memberID;
        public string nickName;
        public string recommenderID;
        public string token;
    }

    public class DWInsUserDataModel
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

        public byte ErrorCode { get; set; }
    }



}