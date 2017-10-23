using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CloudBread.globals;
using System.Data.SqlClient;
using System.Data;

namespace CloudBread
{
    public class DWDataTableManager
    {
        public static Dictionary<string, DataTableListBase> DataTableDic = new Dictionary<string, DataTableListBase>();

        public static bool LoadAllDataTable()
        {
            try
            {
                AddDataTable(BossDataTable_List.NAME, new BossDataTable_List());
                AddDataTable(CaptianDataTable_List.NAME, new CaptianDataTable_List());
                AddDataTable(EnemyDataTable_List.NAME, new EnemyDataTable_List());
                AddDataTable(StageDataTable_List.NAME, new StageDataTable_List());
                AddDataTable(UnitDataTable_List.NAME, new UnitDataTable_List());
                AddDataTable(UnitSummonDataTable_List.NAME, new UnitSummonDataTable_List());
                AddDataTable(WaveDataTable_List.NAME, new WaveDataTable_List());
                AddDataTable(WorldDataTable_List.NAME, new WorldDataTable_List());

                return true;
            }
            catch(Exception)
            {
                throw;
            }
        }

        static void AddDataTable(string tableName, DataTableListBase dataTableListBase)
        {
            LoadDataTable(tableName, dataTableListBase);
            LoadDataVersion(tableName, dataTableListBase);
            DataTableDic.Add(tableName, dataTableListBase);
        }

        static bool LoadDataTable(string tableName, DataTableListBase dataTableListBase)
        {
            SqlConnection conn = new SqlConnection(globalVal.DBConnectionString);

            conn.Open();

            string strQuery = string.Format("SELECT * FROM [{0}DataTable]", tableName);

            SqlCommand command = new SqlCommand(strQuery, conn);

            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(dt);
            }

            dataTableListBase.Load(dt);

            return true;
        }

        static bool LoadDataVersion(string tableName, DataTableListBase dataTableListBase)
        {
            SqlConnection conn = new SqlConnection(globalVal.DBConnectionString);

            conn.Open();

            string strQuery = string.Format("SELECT [Version] FROM [DataVersionTable] WHERE TableName = '{0}'", tableName);

            SqlCommand command = new SqlCommand(strQuery, conn);

            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(dt);
            }

            foreach (DataRow dr in dt.Rows)
            {
                UInt16 version = UInt16.Parse(dr[0].ToString());
                dataTableListBase.Version = version;
            }

            return true;

        }
    }
}