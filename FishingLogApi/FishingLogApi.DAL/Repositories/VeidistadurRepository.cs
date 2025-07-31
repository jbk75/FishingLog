

using Microsoft.Data.SqlClient;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class VeidistadurRepository
{
    public List<FishingPlace> GetVeidistadir()
    {
        string strQuery = @"Select id, name from fishingplace order by name asc";
        SqlCommand cmd = new(strQuery);

        DataTable dt = DatabaseService.GetData(cmd, Constants.connectionString);

        List<FishingPlace> vst = [];
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                FishingPlace item = new FishingPlace();
                item.Name = row["name"].ToString();
                item.Id = Convert.ToInt32(row["id"]);
                vst.Add(item);
            }
        }
        return vst;
    }

    public int AddVeidistadur(string name)
    {
        int rowsAffected = 0;
        string query = @"INSERT INTO FishingPlace (Name, LastModified) VALUES (@name, @lastModified)";

        using (SqlCommand cmd = new(query))
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@lastModified", DateTime.Now);

            rowsAffected = DatabaseService.ExecuteCommand(cmd, Constants.connectionString);
        }
        return rowsAffected;
    }
}
