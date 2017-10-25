using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread.Models
{
    public class DWChangeUnitListInputParam
    {
        public string memberID;
        public string token;
    }

    public class DWChangeUnitListModel
    {
        public List<ulong> UnitList { get; set; }
        public int Gem { get; set; }
        public byte ErrorCode { get; set; }
    }
}