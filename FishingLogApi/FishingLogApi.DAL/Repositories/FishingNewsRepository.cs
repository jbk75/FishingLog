using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using FishingLogApi.DAL.Models;

namespace FishingLogApi.DAL.Repositories;

public sealed class FishingNewsRepository
{
    private readonly string _connectionString;

    public FishingNewsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FishingLogConn");
    }

    public List<FishingNewsDto> GetFishingNews()
    {
        Logger.Logg("Starting, Get Repo - FishingNews...");
        const string query = @"SELECT fn.Id,
                                      fn.FishingPlaceId,
                                      fp.Name AS FishingPlaceName,
                                      fn.[Date],
                                      fn.Description
                               FROM FishingNews fn
                               LEFT JOIN FishingPlace fp ON fn.FishingPlaceId = fp.Id
                               ORDER BY fn.[Date] ASC";
        var cmd = new SqlCommand(query);
        var list = new List<FishingNewsDto>();

        try
        {
            DataTable dt = DatabaseService.GetData(cmd, _connectionString);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var item = new FishingNewsDto
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FishingPlaceId = Convert.ToInt32(row["FishingPlaceId"]),
                        FishingPlaceName = row["FishingPlaceName"]?.ToString(),
                        Date = Convert.ToDateTime(row["Date"]),
                        Description = row["Description"]?.ToString()
                    };
                    list.Add(item);
                }
            }
        }
        catch (Exception)
        {
            return null;
        }

        return list;
    }

    public int AddFishingNews(FishingNewsDto item)
    {
        const string query = @"INSERT INTO FishingNews (FishingPlaceId, [Date], Description)
                               OUTPUT INSERTED.Id
                               VALUES (@FishingPlaceId, @Date, @Description)";

        try
        {
            using SqlCommand cmd = new(query);
            cmd.Parameters.AddWithValue("@FishingPlaceId", item.FishingPlaceId);
            cmd.Parameters.AddWithValue("@Date", item.Date);
            cmd.Parameters.AddWithValue("@Description", item.Description ?? string.Empty);

            return DatabaseService.ExecuteInsertAndReturnId(cmd, _connectionString);
        }
        catch (Exception)
        {
            return -1;
        }
    }

    public bool DeleteFishingNews(int id)
    {
        const string query = @"DELETE FROM FishingNews WHERE Id = @Id";

        try
        {
            using SqlCommand cmd = new(query);
            cmd.Parameters.AddWithValue("@Id", id);

            return DatabaseService.ExecuteCommand(cmd, _connectionString) > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
