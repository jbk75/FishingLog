using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingLogApi.DAL.Repositories
{
    public static class DatabaseService
    {
        /// <summary>
        /// Handles if value is null
        /// </summary>
        /// <param name="value"></param>
        public static object GetColValueString(string value)
        {
            return (value == null) ? (object)DBNull.Value : value;
        }

        public static DataTable GetData(SqlCommand cmd, string connectionString)
        {
            Logger.Logg("Getting data from sql database..");
            DataTable dt = new DataTable();
            //String strConnString = "Data Source=KOBSAPP02;Initial Catalog=KOBS_UserManagement;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection con = new SqlConnection(connectionString);
            SqlDataAdapter sda = new SqlDataAdapter();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            try
            {
                con.Open();
                Logger.Logg("Done opening sql Db connection!");
                sda.SelectCommand = cmd;
                sda.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Logg("Villa kom upp > " +ex.Message);

                return null;
            }
            finally
            {
                con.Close();
                sda.Dispose();
                con.Dispose();
            }
        }
    }
}
