
using FishingLogApi.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

/// <summary>
/// If item contains id we update the record else create new veidiferd
/// </summary>
public class TripRepository
{
    private readonly string _connectionString;

    public TripRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void AddTrip(TripDto item)
    {
        try
        {
            DAL.Logger.Logg("AddVeidiferd");
            string nextId = string.Empty;
            if (item.Id != null)
            {
                UpdateTrip(item);
                return;
            }
            else
            {
                nextId = NextTripId();
            }
            //    Veididagatal_textiRepository vt = new Veididagatal_textiRepository();

            //if (String.IsNullOrWhiteSpace(item.Lysing))
            //{
            //    item.Lysing = "Engin lýsing";
            //}
            //int vet_id = vt.AddVeidiferdTexti(item.Lysing);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new(
                   "INSERT INTO Trip (Id, FishingPlaceId, DateFrom, DateTo, Description) " +
                   "VALUES(@id, @fishingPlaceID, @datefrom, @dateto, @Description)", con))
                {
                    con.Open();
                    cmd.Parameters.Add(new SqlParameter("id", nextId));
                    cmd.Parameters.Add(new SqlParameter("fishingPlaceID", item.VsId));
                    cmd.Parameters.Add(new SqlParameter("datefrom", item.DagsFra));
                    cmd.Parameters.Add(new SqlParameter("dateto", item.DagsTil));
                    cmd.Parameters.Add(new SqlParameter("Description", item.Description));

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                Logger.Logg("AddVeidiferd - Done");
            }
        }
        catch (Exception ex)
        {
            Logger.Logg("Error when adding trip: " + ex.Message);
            throw;
        }
    }

    public void UpdateTrip(Models.TripDto item)
    {
        try
        {
            DAL.Logger.Logg("UpdateVeidiferd");

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                item.Description = "Engin lýsing";
            }
            TripDto? veidiferd = GetTrip(item.Id);

            if (veidiferd == null)
            {
                return;
            }

            using (SqlConnection con = new(_connectionString))
            {
                using (SqlCommand cmd = new(
                   "update trip " +
                   @"set datefrom=@dateFrom, dateto=@dateTo, description=@Description, fishingplaceid=@vsid where id=@id"
                   , con))
                {
                    con.Open();
                    cmd.Parameters.Add(new SqlParameter("id", item.Id));
                    cmd.Parameters.Add(new SqlParameter("datefrom", item.DagsFra));
                    cmd.Parameters.Add(new SqlParameter("dateto", item.DagsTil));
                    cmd.Parameters.Add(new SqlParameter("description", item.Description));
                    cmd.Parameters.Add(new SqlParameter("vsid", item.VsId));

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                DAL.Logger.Logg("Update Veidiferd - Done");
            }
        }
        catch (Exception ex)
        {
            Logger.Logg("Villa kom upp> " + ex.Message);
            throw;
        }
    }

    public string NextTripId()
    {
        int i = -1;
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            con.Open();
            using (SqlCommand cmd = new("SELECT MAX(id) FROM trip", con))
            {
                //Associate connection with your command an open it
                i = (int)cmd.ExecuteScalar();
            }
        }
        return (i + 1).ToString();
    }

    public bool IdExists(string id)
    {
        int i = -1;
        using (SqlConnection con = new(_connectionString))
        {
            con.Open();
            using (SqlCommand cmd = new("SELECT count(*) FROM trip where id = @id", con))
            {
                cmd.Parameters.Add(new SqlParameter("id", id));
                //Associate connection with your command an open it
                i = (int)cmd.ExecuteScalar();
                if (i > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool TripExists(string fishingPlaceName, DateTime dateFrom, DateTime dateTo)
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            con.Open();

            string query = @"
            SELECT COUNT(*) 
            FROM Trip t
            INNER JOIN FishingPlace f ON t.FishingPlaceId = f.Id
            WHERE f.Name = @fishingPlaceName
              AND t.DateFrom = @dateFrom
              AND t.DateTo = @dateTo";

            using (SqlCommand cmd = new(query, con))
            {
                cmd.Parameters.AddWithValue("@fishingPlaceName", fishingPlaceName);
                cmd.Parameters.AddWithValue("@dateFrom", dateFrom);
                cmd.Parameters.AddWithValue("@dateTo", dateTo);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }


    public TripDto? GetTrip(int? id)
    {
        Logger.Logg("Starting, Get Repo - Veidiferd by id " + id + "...");
        string strQuery = @"Select id, fishingplaceid, description, datefrom, dateto from trip where id = @id";

        SqlCommand cmd = new(strQuery);
        cmd.Parameters.Add(new SqlParameter("id", id));

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);

        List<TripDto> list = [];
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                TripDto item = new();
                item.Id = Convert.ToInt32(row["id"]);
                item.VsId = Convert.ToInt32(row["fishingplaceid"]);
                item.Description = row["description"].ToString();
                item.DagsFra = GetDate(row["datefrom"].ToString());
                item.DagsTil = GetDate(row["dateto"].ToString());
                list.Add(item);
            }

        }
        if (list != null && list.Count > 0)
        {
            return list[0];
        }
        return null;

    }


    private DateTime GetDate(string date)
    {
        DateTime goodDate;
        if (DateTime.TryParse(date, out goodDate))
        {
            return goodDate;
        }
        return new DateTime(2999, 1, 1);
    }

    public List<TripDto> GetTrips()
    {
        Logger.Logg("Starting, Get Repo - Veidiferdir...");
        string strQuery = @"Select id, fishingplaceid, description, datefrom, dateto from trip order by datefrom asc";

        SqlCommand cmd = new(strQuery);

        List<TripDto> list = new();
        //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
        try
        {

            DataTable dt = DatabaseService.GetData(cmd, _connectionString);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    TripDto item = new();
                    item.Id = Convert.ToInt32(row["id"]);
                    item.VsId = Convert.ToInt32(row["fishingplaceid"]);
                    item.Description = row["description"].ToString();
                    item.DagsFra = GetDate(row["datefrom"].ToString());
                    item.DagsTil = GetDate(row["dateto"].ToString());
                    list.Add(item);
                }

            }
        }
        catch (Exception ex)
        {
            return null;
            throw;

        }
        return list;
    }

    public List<TripDto> SearchTrips(string searchText)
    {
        var trips = new List<TripDto>();

        using (SqlConnection con = new(_connectionString))
        {
            con.Open();

            string query = @"
            SELECT t.Description, t.DateFrom, t.DateTo, t.DateCreated, t.FishingPlaceId
            FROM Trip t
            INNER JOIN FishingPlace f ON t.FishingPlaceId = f.Id
            WHERE t.Description LIKE @searchText";

            using (SqlCommand cmd = new(query, con))
            {
                // Add wildcards for the LIKE search
                cmd.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var trip = new TripDto
                    {
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DagsFra = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DagsTil = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        Timastimpill = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                        VsId = reader.GetInt32("FishingPlaceId")
                    };

                    trips.Add(trip);
                }
            }
        }

        return trips;
    }

    public async Task UpsertTripsAsync(List<TripDto> clientTrips)
    {
        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();

        foreach (var trip in clientTrips)
        {
            var cmd = new SqlCommand(@"
            MERGE trip AS target
            USING (SELECT @SyncId AS SyncId) AS source
            ON target.SyncId = source.SyncId
            WHEN MATCHED AND target.LastModifiedUtc < @LastModifiedUtc THEN
                UPDATE SET 
                    Description = @Description,
                    Timastimpill = @Timastimpill,
                    DagsFra = @DagsFra,
                    DagsTil = @DagsTil,
                    VsId = @VsId,
                    LastModifiedUtc = @LastModifiedUtc,
                    IsDeleted = @IsDeleted
            WHEN NOT MATCHED THEN
                INSERT (Description, Timastimpill, DagsFra, DagsTil, VsId, SyncId, LastModifiedUtc, IsDeleted)
                VALUES (@Description, @Timastimpill, @DagsFra, @DagsTil, @VsId, @SyncId, @LastModifiedUtc, @IsDeleted);", con);

            cmd.Parameters.AddWithValue("@Description", (object?)trip.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Timastimpill", trip.Timastimpill);
            cmd.Parameters.AddWithValue("@DagsFra", trip.DagsFra);
            cmd.Parameters.AddWithValue("@DagsTil", trip.DagsTil);
            cmd.Parameters.AddWithValue("@VsId", trip.VsId);
            cmd.Parameters.AddWithValue("@SyncId", trip.SyncId);
            cmd.Parameters.AddWithValue("@LastModifiedUtc", trip.LastModifiedUtc);
            cmd.Parameters.AddWithValue("@IsDeleted", trip.IsDeleted);

            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<TripDto>> GetTripsChangedSinceAsync(DateTime lastSyncUtc)
    {
        var trips = new List<TripDto>();

        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();

        var cmd = new SqlCommand(@"
        SELECT Id, Description, Timastimpill, DagsFra, DagsTil, VsId, SyncId, LastModifiedUtc, IsDeleted
        FROM trip
        WHERE LastModifiedUtc > @LastSyncUtc", con);

        cmd.Parameters.AddWithValue("@LastSyncUtc", lastSyncUtc);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trips.Add(new TripDto
            {
                Id = reader.GetInt32(0),
                Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                Timastimpill = reader.GetDateTime(2),
                DagsFra = reader.GetDateTime(3),
                DagsTil = reader.GetDateTime(4),
                VsId = reader.GetInt32(5),
                SyncId = reader.GetGuid(6),
                LastModifiedUtc = reader.GetDateTime(7),
                IsDeleted = reader.GetBoolean(8)
            });
        }

        return trips;
    }


}
