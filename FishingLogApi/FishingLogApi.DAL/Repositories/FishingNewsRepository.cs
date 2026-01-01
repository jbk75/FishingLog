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
        const string query = @"SELECT Id, FishingPlaceId, Date, Description FROM FishingNews ORDER BY Date ASC";
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
}
