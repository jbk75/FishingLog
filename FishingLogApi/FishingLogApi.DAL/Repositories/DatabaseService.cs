﻿
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
}
