using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using NookstreetTurnipMarket.Helper;

namespace NookstreetTurnipMarket.Data
{
    public class DatabaseManager
    {
        #region Connection
        private static SqlConnection m_Connection;

        private static SqlConnection Connection { get { return m_Connection ?? Connect(); } }

        public static SqlConnection Connect()
        {
            m_Connection = new SqlConnection
                (
                    //enter connection string to your SQL Database here
                    string.Empty
                );

            try
            {
                m_Connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening connection, see error: " + e.Message);
            }

            return m_Connection;
        }
        #endregion

        #region Helpers
        private static void ExecuteNonQuery(string aCommand)
        {
            SqlCommand command = new SqlCommand(aCommand, Connection);

            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error executing query: " + aCommand + "\n See error: " + e.Message);
            }
        }

        private static int ExecuteScalar(string aCommand)
        {
            SqlCommand command = new SqlCommand(aCommand, Connection);

            try
            {
                return (int)command.ExecuteScalar();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error executing query: " + aCommand + "\n See error: " + e.Message);
            }

            return 0;
        }
        #endregion

        #region Server Queries
        public static bool TableExists(ulong aDiscordID)
        {
            string command = string.Format("select case when exists((select * from information_schema.tables where table_name = '{0}')) then 1 else 0 end", aDiscordID);

            return ExecuteScalar(command) > 0;
        }

        public static void CreateTable(ulong aDiscordID)
        {
            string command = string.Format
                (
                    "CREATE TABLE \"{0}\" (" +
                    "UserID bigint NOT NULL," +
                    "UserIslandName varchar(45) DEFAULT NULL," +
                    "UserVillagerName varchar(45) DEFAULT NULL," +
                    "UserSellPrice smallint  DEFAULT 0," + 
                    "UserBuyPrice smallint  DEFAULT 0," +
                    "UserLastUpdateIslandTime datetime DEFAULT 0," +
                    "UserLastUpdateServerTime datetime DEFAULT 0,"+
                    "SundayPrice smallint  DEFAULT 0," +
                    "MondayPriceAM smallint DEFAULT 0," +
                    "MondayPricePM smallint DEFAULT 0, " +
                    "TuesdayPriceAM smallint DEFAULT 0," +
                    "TuesdayPricePM smallint DEFAULT 0," +
                    "WednesdayPriceAM smallint DEFAULT 0," +
                    "WednesdayPricePM smallint DEFAULT 0," +
                    "ThursdayPriceAM smallint DEFAULT 0," +
                    "ThursdayPricePM smallint DEFAULT 0," +
                    "FridayPriceAM smallint DEFAULT 0," +
                    "FridayPricePM smallint DEFAULT 0," +
                    "SaturdayPriceAM smallint DEFAULT 0," +
                    "SaturdayPricePM smallint DEFAULT 0," +
                    "PRIMARY KEY(UserID)" +
                    "); ",
                    aDiscordID
                );

            ExecuteNonQuery(command);
        }

        public static void UpdateTable(ulong aDiscordServerID)
        {
            Console.WriteLine("Updating server " + aDiscordServerID);

            string commandString = string.Format("SELECT * FROM \"{0}\" WHERE UserSellPrice > 0 OR UserBuyPrice > 0;", aDiscordServerID);

            SqlCommand command = Connection.CreateCommand();
            command.CommandText = commandString;

            List<long> membersToReset = new List<long>();

            try
            {
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DateTime postedIslandTime = reader.GetDateTime(reader.GetOrdinal("UserLastUpdateIslandTime"));
                    DateTime postedServerTime = reader.GetDateTime(reader.GetOrdinal("UserLastUpdateServerTime"));

                    double difference = (DateTime.Now - postedServerTime).TotalMinutes;

                    if (difference > 600)
                    {
                        membersToReset.Add(reader.GetInt64(reader.GetOrdinal("UserID")));
                    }
                    else
                    {
                        if (postedIslandTime.Hour > 8 && postedIslandTime.Hour < 12)
                        {
                            if (postedIslandTime.AddMinutes(difference).Hour >= 12 || postedIslandTime.Day != DateTime.Now.Day || postedIslandTime.Month != DateTime.Now.Month)
                            {
                                membersToReset.Add(reader.GetInt64(reader.GetOrdinal("UserID")));
                            }
                        }
                        else
                        {
                            if (postedIslandTime.AddMinutes(difference).Hour >= 22 || postedIslandTime.Day != DateTime.Now.Day || postedIslandTime.Month != DateTime.Now.Month)
                            {
                                membersToReset.Add(reader.GetInt64(reader.GetOrdinal("UserID")));
                            }
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Updating database, see error: " + e.Message);
            }

            foreach (ulong id in membersToReset)
            {
                EditCurrentTurnipPrices(aDiscordServerID, id, 0, 0, DateTime.Now); ;
            }

            GuildManager.GuildTableUpdated(aDiscordServerID);

            Console.WriteLine("Server " + aDiscordServerID + " is done updating.");
        }

        #endregion

        #region User Queries
        public static void RegisterUser(ulong aDiscordServerID, ulong aDiscordUserID, string aVillagerName, string aIslandName)
        {
            string commandString = string.Format("INSERT INTO \"{0}\" (UserID, UserVillagerName, UserIslandName) VALUES ({1}, @VillagerName, @IslandName)", aDiscordServerID, aDiscordUserID);

            SqlCommand command = new SqlCommand(commandString, Connection);
            command.Parameters.Add("@VillagerName", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("@IslandName", System.Data.SqlDbType.VarChar);
            command.Parameters["@VillagerName"].Value = aVillagerName;
            command.Parameters["@IslandName"].Value = aIslandName;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error executing query: " + commandString + "\n See error: " + e.Message);
            }
        }

        public static void EditUser(ulong aDiscordServerID, ulong aDiscordUserID, string aVillagerName, string aIslandName)
        {
            string commandString = string.Format
                (
                    "UPDATE \"{0}\" SET UserVillagerName = @VillagerName, UserIslandName = @IslandName WHERE UserId = {1}",
                    aDiscordServerID, aDiscordUserID
                );

            SqlCommand command = new SqlCommand(commandString, Connection);
            command.Parameters.Add("@VillagerName", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("@IslandName", System.Data.SqlDbType.VarChar);
            command.Parameters["@VillagerName"].Value = aVillagerName;
            command.Parameters["@IslandName"].Value = aIslandName;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error executing query: " + commandString + "\n See error: " + e.Message);
            }
        }

        public static bool UserExists(ulong aDiscordServerID, ulong aDiscorderUserID)
        {
            string commandString = string.Format("SELECT COUNT(*) FROM \"{0}\" WHERE UserID = {1}", aDiscordServerID, aDiscorderUserID);

            return ExecuteScalar(commandString) > 0;
        }
        #endregion

        #region Turnip Commands
        public static void EditCurrentTurnipPrices(ulong aDiscordServerID, ulong aDiscordID, int aTurnipBuy, int aTurnipSell, DateTime aIslandTime)
        {
            string command = string.Format
                ("UPDATE \"{0}\" SET UserSellPrice = {1}, UserBuyPrice = {2}, UserLastUpdateIslandTime = '{3}', UserLastUpdateServerTime = '{4}' WHERE UserId = {5}",
                aDiscordServerID,
                aTurnipSell,
                aTurnipBuy,
                TimeHelper.ConvertDateTime(aIslandTime),
                TimeHelper.ConvertDateTime(DateTime.Now),
                aDiscordID);

            ExecuteNonQuery(command);
        }

        public static void SetDayPrice(ulong aDiscordServerID, ulong aDiscordUserID, int aPrice, Day aDay, DayPeriod aPeriod, DateTime aIslandTime)
        {
            int[] currentPrices = GetDayPrices(aDiscordServerID, aDiscordUserID);



            for (int i = (aDay == Day.Sunday ? 0 : ((int)aDay * 2) + (int)aPeriod - 1); i < currentPrices.Length; i++)
            {
                if (currentPrices[i] > 0)
                {
                    ResetDayPrices(aDiscordServerID, aDiscordUserID);
                    break;
                }
            }

            string timeslot = aDay + "Price" + (aDay == Day.Sunday ? string.Empty : aPeriod.ToString());
            string command = string.Format("UPDATE \"{0}\" SET {1} = {2} WHERE UserId = {3}", aDiscordServerID, timeslot, aPrice, aDiscordUserID);

            ExecuteNonQuery(command);
            EditCurrentTurnipPrices(aDiscordServerID, aDiscordUserID, (aDay == Day.Sunday ? aPrice : 0), (aDay == Day.Sunday ? 0 : aPrice), aIslandTime);
        }

        public static void EditDayPrice(ulong aDiscordServerID, ulong aDiscordUserID, int aPrice, Day aDay, DayPeriod aPeriod, DateTime aIslandTime)
        {
            string timeslot = aDay + "Price" + (aDay == Day.Sunday ? string.Empty : aPeriod.ToString());
            string command = string.Format("UPDATE \"{0}\" SET {1} = {2} WHERE UserId = {3}", aDiscordServerID, timeslot, aPrice, aDiscordUserID);

            Console.WriteLine(command);

            ExecuteNonQuery(command);
        }

        public static void ResetDayPrices(ulong aDiscordServerID, ulong aDiscordUserID)
        {
            string command = "UPDATE \"" + aDiscordServerID + "\" SET SundayPrice = 0";

            for (int i = 1; i < 7; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    command += ", " + ((Day)i).ToString() + "Price" + ((DayPeriod)j).ToString() + " = 0";
                }
            }

            command += " WHERE UserID = " + aDiscordUserID;

            ExecuteNonQuery(command);
        }

        public static int[] GetDayPrices(ulong aDiscordServerID, ulong aDiscordUserID)
        {
            int[] prices = new int[13];

            SqlCommand command = Connection.CreateCommand();
            command.CommandText = string.Format("SELECT * FROM \"{0}\" WHERE UserID = {1}", aDiscordServerID, aDiscordUserID);

            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prices[0] = (int)reader.GetInt16(reader.GetOrdinal("SundayPrice"));

                        for (int i = 1; i < 7; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                prices[i + j + (i - 1)] = (int)(reader.GetInt16(reader.GetOrdinal(((Day)i).ToString() + "Price" + ((DayPeriod)j).ToString())));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting day prices, see error: " + e.Message);
            }

            return prices;
        }

        public static string GetTop(ulong aDiscordServerID, string aPriceType, int aTopAmount)
        {
            if ((DateTime.Now - GuildManager.LastUpdate(aDiscordServerID)).TotalMinutes > 5)
            {
                UpdateTable(aDiscordServerID);
            }

            string column = aPriceType == "buy" ? "UserBuyPrice" : "UserSellPrice";

            SqlCommand command = Connection.CreateCommand();

            if (aPriceType == "buy")
            {
                command.CommandText = string.Format("SELECT TOP {1} * FROM \"{0}\" WHERE UserBuyPrice > 0 ORDER BY " + column + " DESC", aDiscordServerID, aTopAmount);
            }
            else
            {
                command.CommandText = string.Format("SELECT TOP {1} * FROM \"{0}\" WHERE UserSellPrice > 0 ORDER BY " + column + " ASC", aDiscordServerID, aTopAmount);
            }

            Console.WriteLine(command.CommandText);

            string topPrices = string.Empty;

            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topPrices += reader.GetString(reader.GetOrdinal("UserVillagerName")) +
                            " has a " + aPriceType + " price of " + reader.GetInt16(reader.GetOrdinal(column)) + 
                            " on their island " + reader.GetString(reader.GetOrdinal("UserIslandName")) + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting top results, see error: " + e.Message);
            }

            return topPrices;
        }
        #endregion
    }
}