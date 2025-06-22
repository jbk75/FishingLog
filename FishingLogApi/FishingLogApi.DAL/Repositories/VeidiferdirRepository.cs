
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

    public VeidiferdirRepository()
    {
    }

    public void AddVeidiferd(Models.Veidiferd item)
    {
        try
        {
            DAL.Logger.Logg("AddVeidiferd");
            string nextId = string.Empty;
            if (!string.IsNullOrWhiteSpace(item.Id))
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
                   "INSERT INTO Trip " +
                   @"VALUES(@id, @fishingplaceID, @datefrom, @dateTo, @Description, @datecreated)"
                   , con))
                {
                    con.Open();
                    cmd.Parameters.Add(new SqlParameter("id", nextId));
                    cmd.Parameters.Add(new SqlParameter("fishingPlaceID", item.VsId));
                    cmd.Parameters.Add(new SqlParameter("datefrom", item.DagsFra));
                    cmd.Parameters.Add(new SqlParameter("dateto", item.DagsTil));
                    cmd.Parameters.Add(new SqlParameter("Description", item.Lysing));
                    //var lysingShort = String.Empty;
                    //if (item.Lysing.Length > 25)
                    //{
                    //    lysingShort = item.Lysing.Substring(0, 24) + "...";
                    //}
                    //else
                    //{
                    //    lysingShort = item.Lysing.Substring(0, item.Lysing.Length - 1) + "...";
                    //}

                    //cmd.Parameters.Add(new SqlParameter("lysing", lysingShort));

                    //CultureInfo isl = new CultureInfo("is-IS");
                    //DateTime myDate = DateTime.Parse(DateTime.Now, isl.DateTimeFormat);
                    ////cmd.Parameters.Add(new SqlParameter("entryoccurred", driveEventItem.Entry.occurred));
                    //SqlParameter entryoccurredDateTimeParam = new SqlParameter("@entryoccurred", SqlDbType.DateTime);
                    //entryoccurredDateTimeParam.Value = myDate;
                    //cmd.Parameters.Add(entryoccurredDateTimeParam);

                    //cmd.Parameters.Add(new SqlParameter("Ar", item.DagsFra.Year.ToString()));
                    //cmd.Parameters.Add(new SqlParameter("vet_id", vet_id));
                    //cmd.Parameters.Add(new SqlParameter("vsid", item.VsId));
                    //cmd.Parameters.Add(new SqlParameter("koid", "-1")); // item.KoId));

                    cmd.Parameters.Add(new SqlParameter("DateCreated", DateTime.Now));
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                DAL.Logger.Logg("AddVeidiferd - Done");
            }
        }
        catch (Exception ex)
        {
            Logger.Logg("Villa kom upp> " + ex.Message);
            throw;
        }
    }

    public void UpdateVeidiferd(Models.Veidiferd item)
    {
        try
        {
            DAL.Logger.Logg("UpdateVeidiferd");

            //Veididagatal_textiRepository vt = new Veididagatal_textiRepository();

            if (String.IsNullOrWhiteSpace(item.Lysing))
            {
                item.Lysing = "Engin lýsing";
            }
            var veidiferd = GetVeidiferd(item.Id);
            //vt.UpdateVeidiferdTexti(item.Lysing, veidiferd.VetId);

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

                    cmd.Parameters.Add(new SqlParameter("description", item.Lysing));

                    //cmd.Parameters.Add(new SqlParameter("Ar", item.DagsFra.Year.ToString()));
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

    public Veidiferd GetVeidiferd(string id)
    {
        Logger.Logg("Starting, Get Repo - Veidiferd by id " + id + "...");
        string strQuery = @"Select id, fishingplaceid, description, datefrom, dateto from trip where id = @id";

        SqlCommand cmd = new(strQuery);
        cmd.Parameters.Add(new SqlParameter("id", id));

        DataTable dt = DatabaseService.GetData(cmd, _connectionString);

        List<Veidiferd> list = new List<Veidiferd>();
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                Veidiferd item = new Veidiferd();
                item.Id = row["id"].ToString();
                item.VsId = row["fishingplaceid"].ToString();
                item.Lysing = row["description"].ToString();
                item.DagsFra = GetDate(row["datefrom"].ToString());
                item.DagsTil = GetDate(row["dateto"].ToString());
                //item.VetId = row["vet_id"].ToString();

                Veididagatal_textiRepository repoText = new Veididagatal_textiRepository();
                //string text = string.Empty;
                //if (!string.IsNullOrWhiteSpace(row["vet_id"].ToString()))
                //{ 
                //    text = repoText.GetVeidiferdTexti(row["vet_id"].ToString());
                //}
                item.LysingLong = item.Lysing;

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

        SqlCommand cmd = new SqlCommand(strQuery);

        List<Veidiferd> list = new List<Veidiferd>();
        //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
        try
        {

            DataTable dt = DatabaseService.GetData(cmd, _connectionString);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Veidiferd item = new Veidiferd();
                    item.Id = row["id"].ToString();
                    item.VsId = row["fishingplaceid"].ToString();
                    item.Lysing = row["description"].ToString();
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

}
