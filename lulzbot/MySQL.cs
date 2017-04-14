using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace lulzbot
{
    public class MySQL
    {
        public static MySqlConnection CreateConnection (String connection_string)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(connection_string);
                conn.Open();
                return conn;
            }
            catch (Exception E)
            {
                ConIO.Warning("MySQL", "Failed to create connection: " + E.Message);
                return null;
            }
        }

        public static MySqlConnection CreateConnection (String username, String password, String database, String hostname, int port = 3306)
        {

            try
            {
                MySqlConnection conn = new MySqlConnection(String.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}", hostname, port, database, username, password));
                conn.Open();
                return conn;
            }
            catch (Exception E)
            {
                ConIO.Warning("MySQL", "Failed to create connection: " + E.Message);
                return null;
            }
        }

        public static bool SelectDatabase (MySqlConnection connection, String database)
        {
            try
            {
                connection.ChangeDatabase(database);
                return true;
            }
            catch (Exception E)
            {
                ConIO.Warning("MySQL", "Failed to change database: " + E.Message);
                return false;
            }
        }

        public static void CloseConnection (MySqlConnection connection)
        {
            try
            {
                connection.Close();
                connection.Dispose();
            }
            catch { }
        }

        public static bool NonQuery (MySqlConnection connection, String query)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception E)
            {
                ConIO.Warning("MySQL", "Failed to perform non-query: " + E.Message);
                return false;
            }
        }

        public static List<KeyValuePair<String, Object>> Query (MySqlConnection connection, String query)
        {
            try
            {
                var ret = new List<KeyValuePair<String, Object>>();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;

                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                ret.Add(new KeyValuePair<String, Object>(reader.GetName(i), reader.GetValue(i)));
                            }
                        }
                    }
                }

                return ret;
            }
            catch (Exception E)
            {
                ConIO.Warning("MySQL", "Failed to perform query: " + E.Message);
                return null;
            }
        }
    }
}
