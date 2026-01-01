

using Microsoft.Data.SqlClient;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class Veididagatal_textiRepository
{
    public int AddVeidiferdTexti(string texti)
    {
        var nextId = NextId();

        using (SqlConnection con = new SqlConnection(Constants.connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(
               "INSERT INTO Veididagatal_texti " +
                    @"VALUES(@vet_id, @vet_texti)", con))
            {
                con.Open();
                cmd.Parameters.Add(new SqlParameter("vet_id", nextId));
                cmd.Parameters.Add(new SqlParameter("vet_texti", texti));

                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
        return nextId;
    }

    public string GetVeidiferdTexti(string vetid)
    {
        Logger.Logg("Starting, Get Repo - VeidiferdTexti by id " + vetid + "...");
        string strQuery = @"Select vet_texti from Veididagatal_texti where vet_id = @id";

        SqlCommand cmd = new SqlCommand(strQuery);
        cmd.Parameters.Add(new SqlParameter("id", vetid));
        var connectionString = Constants.connectionString;
        DataTable dt = DatabaseService.GetData(cmd, connectionString);

        string texti = string.Empty;
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                texti = row["vet_texti"].ToString();
            }

        }
        return texti;
    }

    public void UpdateVeidiferdTexti(string texti, string vetid)
    {
        try
        {

        using (SqlConnection con = new SqlConnection(Constants.connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(
               "update Veididagatal_texti " +
                    @"set vet_texti=@vet_texti where vet_id=@vetid", con))
            {
                con.Open();
                cmd.Parameters.Add(new SqlParameter("vetid", vetid));
                cmd.Parameters.Add(new SqlParameter("vet_texti", texti));

                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
        }
        catch (Exception ex)
        {
            Logger.Logg("Villa kom upp> " + ex.Message);
            throw;
        }

    }

    public int NextId()
    {
        int i = -1;
        using (SqlConnection con = new SqlConnection(Constants.connectionString))
        {
            con.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT MAX(vet_id) FROM veididagatal_texti", con))
            {
                //Associate connection with your command an open it
                i = (int)cmd.ExecuteScalar();
            }
        }
        return i + 1;
    }
}
