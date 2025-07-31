
using Microsoft.Data.SqlClient;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

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
        //Logger.Logg("Getting data from sql database..");
        DataTable dt = new DataTable();

        SqlConnection con = new(connectionString);
        SqlDataAdapter sda = new();
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

    public static int ExecuteCommand(SqlCommand cmd, string connectionString)
    {
        int rowsAffected = 0;

        using (SqlConnection con = new(connectionString))
        {
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            try
            {
                con.Open();
                Logger.Logg("Executing non-query SQL command...");
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Logg("Error executing SQL command: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        return rowsAffected;
    
    }


    /// <summary>
    /// If you want to return the newly inserted Id, use this
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static int ExecuteInsertAndReturnId(SqlCommand cmd, string connectionString)
    {
        int insertedId = 0;

        using (SqlConnection con = new(connectionString))
        {
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;

            try
            {
                con.Open();
                Logger.Logg("Executing insert and retrieving new ID...");
                object result = cmd.ExecuteScalar();
                insertedId = Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.Logg("Error executing insert: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        return insertedId;
    }


}
