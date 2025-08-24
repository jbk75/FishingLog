
using FishingLogApi.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

/// <summary>
/// If item contains id we update the record else create new veidiferd
/// </summary>
public class VeidiferdirRepository
{
    private readonly string _connectionString;

    public VeidiferdirRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void AddVeidiferd(Veidiferd item)
    {
        try
        {
            DAL.Logger.Logg("AddVeidiferd");
            string nextId = string.Empty;
            if (item.Id != null)
            {
                UpdateVeidiferd(item);
                return;
            }
            else
            {
                nextId = NextVeidiferdId();
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

    public void UpdateVeidiferd(Models.Veidiferd item)
    {
        try
        {
            DAL.Logger.Logg("UpdateVeidiferd");

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                item.Description = "Engin lýsing";
            }
            Veidiferd? veidiferd = GetVeidiferd(item.Id);

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

    public string NextVeidiferdId()
    {
        int i = -1;
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            con.Open();
            using (SqlCommand cmd = new SqlCommand("SELECT MAX(id) FROM trip", con))
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

    public bool VeidiferdExists(string fishingPlaceName, DateTime dateFrom, DateTime dateTo)
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


    public Veidiferd? GetVeidiferd(int? id)
    {
        Logger.Logg("Starting, Get Repo - Veidiferd by id " + id + "...");
        string strQuery = @"Select id, fishingplaceid, description, datefrom, dateto from trip where id = @id";

        SqlCommand cmd = new(strQuery);
        cmd.Parameters.Add(new SqlParameter("id", id));

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);

        List<Veidiferd> list = [];
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                Veidiferd item = new Veidiferd();
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

    public List<Veidiferd> GetVeidiferdir()
    {
        Logger.Logg("Starting, Get Repo - Veidiferdir...");
        string strQuery = @"Select id, fishingplaceid, description, datefrom, dateto from trip order by datefrom asc";

        SqlCommand cmd = new(strQuery);

        List<Veidiferd> list = new();
        //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
        try
        {

            DataTable dt = DatabaseService.GetData(cmd, _connectionString);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Veidiferd item = new Veidiferd();
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

    public List<Veidiferd> SearchTrips(string searchText)
    {
        var trips = new List<Veidiferd>();

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
                    var trip = new Veidiferd
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

}
