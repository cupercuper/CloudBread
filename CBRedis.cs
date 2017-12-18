/**
* @file CBRedis.cs
* @brief Processing CloudBread redis cache related task. \n
* @author Dae Woo Kim
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CloudBread.globals;
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using CloudBread;
//using Logger.Logging;

namespace CloudBreadRedis
{
    public class CBRedis
    {
        // compose connection string for service
        static string redisConnectionStringSocket = globalVal.CloudBreadSocketRedisServer;
        static string redisConnectionStringRank = globalVal.CloudBreadRankRedisServer;
        static string CloudBreadGameLogRedisServer = globalVal.CloudBreadGameLogRedisServer;
        static string EMPTY_USER = "Master";

        /// @brief save socket auth key in redis db0
        public static bool SetRedisKey(string key, string value, int? expTimeMin)    // todo: value as oject or ...?
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringSocket);

            // try to connect database
            try
            {
                // StringSet task
                IDatabase cache = connection.GetDatabase(0);
                if (expTimeMin == null)
                {
                    // save without expire time
                    cache.StringSet(key, value);
                }
                else
                {
                    cache.StringSet(key, value, TimeSpan.FromMinutes(Convert.ToDouble(expTimeMin)));
                }

                connection.Close();
                connection.Dispose();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// @brief get socket auth key redis data by key value
        public static string GetRedisKeyValue(string key)
        {
            string result = "";
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringSocket);

            // try to connect database
            try
            {
                // StringGet task
                IDatabase cache = connection.GetDatabase(0);
                result = cache.StringGet(key);

                connection.Close();
                connection.Dispose();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// @brief Set point value at Redis sorted set
        public static bool SetSortedSetRank(string sid, double point)
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);

            try
            {
                IDatabase cache = connection.GetDatabase(1);
                cache.SortedSetAdd(globalVal.CloudBreadRankSortedSet, sid, point);

                connection.Close();
                connection.Dispose();

            }
            catch (Exception)
            {

                throw;
            }

            return true;
        }

        /// @brief Get rank value from Redis sorted set
        public static long GetSortedSetRank(string sid)
        {
            long rank = 0;
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);

            try
            {
                IDatabase cache = connection.GetDatabase(1);
                rank = cache.SortedSetRank(globalVal.CloudBreadRankSortedSet, sid, Order.Descending) ?? 0;

                connection.Close();
                connection.Dispose();

            }
            catch (Exception)
            {

                throw;
            }

            return rank;
        }

        /// @brief Get rank value from Redis sorted set
        public static double GetSortedSetScore(string sid)
        {
            double score = 0;
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);

            try
            {
                IDatabase cache = connection.GetDatabase(1);
                score = cache.SortedSetScore(globalVal.CloudBreadRankSortedSet, sid) ?? 0;

                connection.Close();
                connection.Dispose();

            }
            catch (Exception)
            {

                throw;
            }

            return score;
        }


        /// @brief Get selected rank range members. 
        /// Get my rank and then call this method to fetch +-10 rank(total 20) rank
        public static SortedSetEntry[] GetSortedSetRankByRange(long startRank, long endRank)
        {

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);

            try
            {
                IDatabase cache = connection.GetDatabase(1);
                //SortedSetEntry[] rank = cache.SortedSetRangeByScoreWithScores(globalVal.CloudBreadRankSortedSet, startRank, endRank, Exclude.None, Order.Descending);
                SortedSetEntry[] se = cache.SortedSetRangeByRankWithScores(globalVal.CloudBreadRankSortedSet, startRank, endRank, Order.Descending);
                //return JsonConvert.SerializeObject(se);

                connection.Close();
                connection.Dispose();

                return se;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// @brief Get top rank point and info from Redis sorted set
        public static SortedSetEntry[] GetTopSortedSetRank(int countNumber)
        {

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);

            try
            {
                IDatabase cache = connection.GetDatabase(1);
                SortedSetEntry[] sse = cache.SortedSetRangeByScoreWithScores(globalVal.CloudBreadRankSortedSet, order: Order.Descending, take: countNumber);

                connection.Close();
                connection.Dispose();

                return sse;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public static long GetRankCount()
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);
            
            long count = 0;
            try
            {
                IDatabase cache = connection.GetDatabase(1);
                count = cache.SortedSetRank(globalVal.CloudBreadRankSortedSet, EMPTY_USER, Order.Descending) ?? 0;

                connection.Close();
                connection.Dispose();

            }
            catch (Exception)
            {

                throw;
            }

            return count;
        }

        /// fill out all rank redis cache from db
        /// @todo: huge amount of data processing - split 10,000 or ...
        /// dt.Rows check. if bigger than 10,000, seperate as another loop 
        /// dt.Rows / 10,000 = mod value + 1 = loop count...........
        /// call count query first and then paging processing at query side to prevent DB throttling? 
        public static bool FillAllRankFromDB()
        {

            try
            {
                // redis connection
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionStringRank);
                IDatabase cache = connection.GetDatabase(1);

                // delete rank sorted set - caution. this process remove all rank set data
                cache.KeyDelete(globalVal.CloudBreadRankSortedSet);

                // data table fill for easy count number
                RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
                SqlConnection conn = new SqlConnection(globalVal.DBConnectionString);
                conn.Open();
                string strQuery = "SELECT MemberID, CaptianChange, LastWorld FROM DWMembers";

                SqlCommand command = new SqlCommand(strQuery, conn);

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    da.Fill(dt);
                }

                /// make SortedSetEntry to fill out
                SortedSetEntry[] sse = new SortedSetEntry[dt.Rows.Count + 1];
                Int64 i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    string id = (string)dr[0];
                    long captianChange = (long)dr[1];
                    short lastWorld = (short)dr[2];

                    // fill rank row to redis struct array
                    sse[i] = new SortedSetEntry(id, DWMemberData.GetPoint(lastWorld, captianChange));
                    i++;
                }

                // 무조건 -1.0점 유저를 넣어서 Rank의 카운트를 얻어온다
                sse[i] = new SortedSetEntry(EMPTY_USER, -1.0);

                // fill out all rank data
                cache.SortedSetAdd(globalVal.CloudBreadRankSortedSet, sse);

                connection.Close();
                connection.Dispose();

                return true;
            }

            catch (Exception)
            {

                throw;
            }
        }

        /// save log to redis db2 and keep 365 days 
        public static void saveRedisLog(string key, string message, int? expTimeDays)
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(CloudBreadGameLogRedisServer);

            // try to connect database
            try
            {
                // StringSet task
                IDatabase cache = connection.GetDatabase(2);
                if (expTimeDays == null)
                {
                    // save without expire time
                    cache.StringSetAsync(key, message);
                }
                else
                {
                    cache.StringSetAsync(key, message, TimeSpan.FromDays(Convert.ToDouble(expTimeDays)));
                }

                connection.Close();
                connection.Dispose();

            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}