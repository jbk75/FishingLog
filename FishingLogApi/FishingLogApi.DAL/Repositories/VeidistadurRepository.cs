

using FishingLogApi.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class VeidistadurRepository
{
    private readonly string _connectionString;

    public VeidistadurRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? Constants.connectionString;
    }

    public List<FishingPlace> GetVeidistadir()
    {
        const string strQuery = @"Select id, name, FishingPlaceTypeId, Description from fishingplace order by name asc";
        SqlCommand cmd = new(strQuery);

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);

        List<FishingPlace> vst = [];
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                FishingPlace item = new FishingPlace
                {
                    Name = row["name"].ToString(),
                    Id = Convert.ToInt32(row["id"]),
                    FishingPlaceTypeID = row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]),
                    Description = row["Description"]?.ToString() ?? string.Empty
                };
                vst.Add(item);
            }
        }
        return vst;
    }

    public int AddVeidistadur(FishingPlace fishingPlace)
    {
        const string query = @"INSERT INTO FishingPlace (Name, FishingPlaceTypeId, longitude, latitude, NumberOfSpots, Description, LastModified) 
                OUTPUT INSERTED.Id
                VALUES (@name, @FishingPlaceTypeId, @longitude, @latitude, @NumberOfSpots, @Description, @lastModified)";

        try
        {
            using SqlCommand cmd = new(query);
            cmd.Parameters.AddWithValue("@name", fishingPlace.Name);
            cmd.Parameters.AddWithValue("@FishingPlaceTypeId", fishingPlace.FishingPlaceTypeID);
            cmd.Parameters.AddWithValue("@longitude", fishingPlace.Longitude);
            cmd.Parameters.AddWithValue("@Latitude", fishingPlace.Latitude);
            cmd.Parameters.AddWithValue("@NumberOfSpots", fishingPlace.NumberOfSpots);
            cmd.Parameters.AddWithValue("@Description", fishingPlace.Description);

            cmd.Parameters.AddWithValue("@lastModified", DateTime.Now);

            return DatabaseService.ExecuteInsertAndReturnId(cmd, _connectionString);
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public FishingPlace? GetByName(string name)
    {
        const string query = @"Select top 1 id, name, FishingPlaceTypeId, Description from fishingplace where name = @name";

        SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@name", name);

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);
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

    public FishingPlace? GetById(int id)
    {
        const string query = @"Select top 1 id, name, FishingPlaceTypeId, Description from fishingplace where id = @id";

        SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@id", id);

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);
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

    public FishingPlaceRelationCounts GetRelationCounts(int fishingPlaceId)
    {
        const string query = @"
            SELECT
                (SELECT COUNT(*) FROM FishingNews WHERE FishingPlaceId = @id) AS FishingNewsCount,
                (SELECT COUNT(*) FROM FishingPlaceSpot WHERE FishingPlaceId = @id) AS FishingPlaceSpotCount,
                (SELECT COUNT(*) FROM Trip WHERE FishingPlaceId = @id) AS TripCount";

        SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@id", fishingPlaceId);

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);
        if (dt == null || dt.Rows.Count == 0)
        {
            return new FishingPlaceRelationCounts();
        }

        DataRow row = dt.Rows[0];
        return new FishingPlaceRelationCounts
        {
            FishingNewsCount = row["FishingNewsCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingNewsCount"]),
            FishingPlaceSpotCount = row["FishingPlaceSpotCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceSpotCount"]),
            TripCount = row["TripCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["TripCount"])
        };
    }

    public bool DeleteVeidistadur(int id)
    {
        const string query = @"DELETE FROM FishingPlace WHERE Id = @id";
        using SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@id", id);

        int rowsAffected = DatabaseService.ExecuteCommand(cmd, _connectionString);
        return rowsAffected > 0;
    }
}
