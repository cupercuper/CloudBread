using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudBread
{
    public enum DW_ERROR_CODE
    {
        OK = 0,
        NOT_FOUND_USER,
    }

    public enum MONEY_TYPE
    {
        GOLD_TYPE = 0,
        GEM_TYPE,
        ENHANCEMENT_TYPE,
    }
}