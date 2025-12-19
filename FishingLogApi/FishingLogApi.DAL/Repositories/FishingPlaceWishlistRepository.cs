
using FishingLogApi.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class FishingPlaceWishlistRepository
{
    private readonly string _connectionString;

    public FishingPlaceWishlistRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? Constants.connectionString;
    }

    public List<FishingPlaceWishlistItem> GetWishlist()
    {
        const string query = @"
            SELECT fpw.Id,
                   fpw.FishingPlaceId,
                   fpw.Description,
                   fp.Name,
                   fp.FishingPlaceTypeId
            FROM FishingPlaceWishlist fpw
            INNER JOIN FishingPlace fp ON fpw.FishingPlaceId = fp.Id
            ORDER BY fp.Name ASC";

        SqlCommand cmd = new(query);
        DataTable dt = DatabaseService.GetData(cmd, _connectionString);

        List<FishingPlaceWishlistItem> list = [];
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                FishingPlaceWishlistItem item = new()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    FishingPlaceId = Convert.ToInt32(row["FishingPlaceId"]),
                    Description = row["Description"]?.ToString() ?? string.Empty,
                    FishingPlaceName = row["Name"]?.ToString() ?? string.Empty,
                    FishingPlaceTypeId = row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]),
                    FishingPlaceTypeName = GetFishingPlaceTypeName(row["FishingPlaceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["FishingPlaceTypeId"]))
                };

                list.Add(item);
            }
        }

        return list;
    }

    public int AddWishlistItem(FishingPlaceWishlistItem item)
    {
        const string query = @"
            INSERT INTO FishingPlaceWishlist (FishingPlaceId, Description)
            OUTPUT INSERTED.Id
            VALUES (@FishingPlaceId, @Description)";

        try
        {
            using SqlCommand cmd = new(query);
            cmd.Parameters.AddWithValue("@FishingPlaceId", item.FishingPlaceId);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? string.Empty);

            return DatabaseService.ExecuteInsertAndReturnId(cmd, _connectionString);
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private static string GetFishingPlaceTypeName(int typeId)
    {
        return typeId switch
        {
            2 => "River",
            1 => "Lake",
            _ => "Unknown"
        };
    }
}
