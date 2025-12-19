

using Microsoft.Data.SqlClient;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class VeidistadurRepository
{
    public List<FishingPlace> GetVeidistadir()
    {
        string strQuery = @"Select id, name, FishingPlaceTypeId, Description from fishingplace order by name asc";
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
                item.FishingPlaceTypeID = row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]);
                item.Description = row["Description"]?.ToString() ?? string.Empty;
                vst.Add(item);
            }
        }
        return vst;
    }

    public static int AddVeidistadur(FishingPlace fishingPlace)
    {
        int insertedId = 0;
        string query = @"INSERT INTO FishingPlace (Name, FishingPlaceTypeId, longitude, latitude, NumberOfSpots, Description, LastModified) 
                OUTPUT INSERTED.Id
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

                insertedId = DatabaseService.ExecuteInsertAndReturnId(cmd, Constants.connectionString);
            }
        }
        catch(Exception)
        {
            return -1;
        }
        return insertedId;
    }

    public static FishingPlace? GetByName(string name)
    {
        const string query = @"Select top 1 id, name, FishingPlaceTypeId, Description from fishingplace where name = @name";

        SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@name", name);

        DataTable dt = DatabaseService.GetData(cmd, Constants.connectionString);
        if (dt == null || dt.Rows.Count == 0)
        {
            return null;
        }

        DataRow row = dt.Rows[0];
        return new FishingPlace
        {
            Id = Convert.ToInt32(row["id"]),
            Name = row["name"].ToString(),
            FishingPlaceTypeID = row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]),
            Description = row["Description"]?.ToString() ?? string.Empty
        };
    }

    public static FishingPlace? GetById(int id)
    {
        const string query = @"Select top 1 id, name, FishingPlaceTypeId, Description from fishingplace where id = @id";

        SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@id", id);

        DataTable dt = DatabaseService.GetData(cmd, Constants.connectionString);
        if (dt == null || dt.Rows.Count == 0)
        {
            return null;
        }

        DataRow row = dt.Rows[0];
        return new FishingPlace
        {
            Id = Convert.ToInt32(row["id"]),
            Name = row["name"].ToString(),
            FishingPlaceTypeID = row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]),
            Description = row["Description"]?.ToString() ?? string.Empty
        };
    }
}
