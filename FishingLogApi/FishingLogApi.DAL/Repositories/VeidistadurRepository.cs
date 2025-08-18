

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

    public static int AddVeidistadur(FishingPlace fishingPlace)
    {
        int rowsAffected = 0;
        string query = @"INSERT INTO FishingPlace (Name, FishingPlaceTypeId, longitude, latitude, NumberOfSpots, Description, LastModified) 
                VALUES (@name, @FishingPlaceTypeId, @longitude, @latitude, @NumberOfSpots, @Description, @lastModified)";

        try
        {
            using (SqlCommand cmd = new(query))
            {
                cmd.Parameters.AddWithValue("@name", fishingPlace.Name);
                cmd.Parameters.AddWithValue("@FishingPlaceTypeId", fishingPlace.FishingPlaceTypeID);
                cmd.Parameters.AddWithValue("@longitude", fishingPlace.Longitude);
                cmd.Parameters.AddWithValue("@Latitude", fishingPlace.Latitude);
                cmd.Parameters.AddWithValue("@NumberOfSpots", fishingPlace.NumberOfSpots);
                cmd.Parameters.AddWithValue("@Description", fishingPlace.Description);

                cmd.Parameters.AddWithValue("@lastModified", DateTime.Now);

                rowsAffected = DatabaseService.ExecuteCommand(cmd, Constants.connectionString);
            }
        }
        catch(Exception)
        {
            return -1;
        }
        return rowsAffected;
    }
}
