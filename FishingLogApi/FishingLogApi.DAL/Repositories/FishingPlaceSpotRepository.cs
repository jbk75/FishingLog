using System.Data;
using FishingLogApi.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FishingLogApi.DAL.Repositories;

public class FishingPlaceSpotRepository
{
    private readonly string _connectionString;

    public FishingPlaceSpotRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? Constants.connectionString;
    }

    public List<FishingPlaceSpotDto> GetSpots()
    {
        const string query = @"
SELECT fps.Id,
       fps.FishingPlaceId,
       fps.Name,
       fps.Description,
       fps.LastModified,
       fp.Name AS FishingPlaceName
FROM FishingPlaceSpot fps
INNER JOIN FishingPlace fp ON fp.Id = fps.FishingPlaceId
ORDER BY fp.Name ASC, fps.Name ASC";

        using SqlCommand cmd = new(query);
        return ReadSpots(DatabaseService.GetData(cmd, _connectionString));
    }

    public FishingPlaceSpotDto? GetSpotById(int id)
    {
        const string query = @"
SELECT fps.Id,
       fps.FishingPlaceId,
       fps.Name,
       fps.Description,
       fps.LastModified,
       fp.Name AS FishingPlaceName
FROM FishingPlaceSpot fps
INNER JOIN FishingPlace fp ON fp.Id = fps.FishingPlaceId
WHERE fps.Id = @id";

        using SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@id", id);

        List<FishingPlaceSpotDto> spots = ReadSpots(DatabaseService.GetData(cmd, _connectionString));
        return spots.FirstOrDefault();
    }

    private static List<FishingPlaceSpotDto> ReadSpots(DataTable dt)
    {
        List<FishingPlaceSpotDto> spots = [];

        if (dt == null || dt.Rows.Count == 0)
        {
            return spots;
        }

        foreach (DataRow row in dt.Rows)
        {
            FishingPlaceSpotDto spot = new()
            {
                Id = Convert.ToInt32(row["Id"]),
                FishingPlaceId = Convert.ToInt32(row["FishingPlaceId"]),
                Name = row["Name"].ToString() ?? string.Empty,
                Description = row["Description"].ToString(),
                LastModified = Convert.ToDateTime(row["LastModified"]),
                FishingPlaceName = row["FishingPlaceName"].ToString() ?? string.Empty
            };

            spots.Add(spot);
        }

        return spots;
    }

    public bool SpotExists(int fishingPlaceId, string name)
    {
        const string query = @"SELECT COUNT(*) FROM FishingPlaceSpot WHERE FishingPlaceId = @fishingPlaceId AND LOWER(Name) = LOWER(@name)";

        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new(query, con);

        cmd.Parameters.AddWithValue("@fishingPlaceId", fishingPlaceId);
        cmd.Parameters.AddWithValue("@name", name);

        con.Open();
        int count = (int)cmd.ExecuteScalar();
        return count > 0;
    }

    public int AddSpot(CreateFishingPlaceSpotRequest request)
    {
        const string query = @"INSERT INTO FishingPlaceSpot (FishingPlaceId, Name, Description, LastModified)
OUTPUT INSERTED.Id
VALUES (@fishingPlaceId, @name, @description, @lastModified)";

        using SqlCommand cmd = new(query);
        cmd.Parameters.AddWithValue("@fishingPlaceId", request.FishingPlaceId);
        cmd.Parameters.AddWithValue("@name", request.Name);
        string? description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description;
        cmd.Parameters.AddWithValue("@description", DatabaseService.GetColValueString(description));
        cmd.Parameters.AddWithValue("@lastModified", DateTime.UtcNow);

        return DatabaseService.ExecuteInsertAndReturnId(cmd, _connectionString);
    }

    public bool FishingPlaceExists(int fishingPlaceId)
    {
        const string query = "SELECT COUNT(*) FROM FishingPlace WHERE Id = @id";

        using SqlConnection con = new(_connectionString);
        using SqlCommand cmd = new(query, con);

        cmd.Parameters.AddWithValue("@id", fishingPlaceId);

        con.Open();
        int count = (int)cmd.ExecuteScalar();
        return count > 0;
    }
}
