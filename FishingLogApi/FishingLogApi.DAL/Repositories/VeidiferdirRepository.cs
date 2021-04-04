using FishingLogApi.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FishingLogApi.DAL.Repositories
{
    /// <summary>
    /// If item contains id we update the record else create new veidiferd
    /// </summary>
    public class VeidiferdirRepository
    {
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
                Veididagatal_textiRepository vt = new Veididagatal_textiRepository();

            if (String.IsNullOrWhiteSpace(item.Lysing))
            {
                item.Lysing = "Engin lýsing";
            }
            int vet_id = vt.AddVeidiferdTexti(item.Lysing);

                using (SqlConnection con = new SqlConnection(Constants.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                       "INSERT INTO Veidiferdir " +
                       @"VALUES(@id, @dags_fra, @dags_til, @lysing, @timastimpill, @ar, @vet_id, @vsid, @koid)"
                       , con))
                    {
                        con.Open();
                        cmd.Parameters.Add(new SqlParameter("id", nextId));
                        cmd.Parameters.Add(new SqlParameter("dags_fra", item.DagsFra));
                        cmd.Parameters.Add(new SqlParameter("dags_til", item.DagsTil));
                        var lysingShort = String.Empty;
                        if (item.Lysing.Length > 25)
                        {
                            lysingShort = item.Lysing.Substring(0, 24) + "...";
                        }
                        else
                        {
                            lysingShort = item.Lysing.Substring(0, item.Lysing.Length - 1) + "...";
                        }

                        cmd.Parameters.Add(new SqlParameter("lysing", lysingShort));

                        //CultureInfo isl = new CultureInfo("is-IS");
                        //DateTime myDate = DateTime.Parse(DateTime.Now, isl.DateTimeFormat);
                        ////cmd.Parameters.Add(new SqlParameter("entryoccurred", driveEventItem.Entry.occurred));
                        //SqlParameter entryoccurredDateTimeParam = new SqlParameter("@entryoccurred", SqlDbType.DateTime);
                        //entryoccurredDateTimeParam.Value = myDate;
                        //cmd.Parameters.Add(entryoccurredDateTimeParam);
                        cmd.Parameters.Add(new SqlParameter("timastimpill", DateTime.Now));
                        cmd.Parameters.Add(new SqlParameter("Ar", item.DagsFra.Year.ToString()));
                        cmd.Parameters.Add(new SqlParameter("vet_id", vet_id));
                        cmd.Parameters.Add(new SqlParameter("vsid", item.VsId));
                        cmd.Parameters.Add(new SqlParameter("koid", "-1")); // item.KoId));

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

                Veididagatal_textiRepository vt = new Veididagatal_textiRepository();

                if (String.IsNullOrWhiteSpace(item.Lysing))
                {
                    item.Lysing = "Engin lýsing";
                }
                var veidiferd = GetVeidiferd(item.Id);
                vt.UpdateVeidiferdTexti(item.Lysing, veidiferd.VetId);

                using (SqlConnection con = new SqlConnection(Constants.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                       "update Veidiferdir " +
                       @"set dags_fra=@dags_fra, dags_til=@dags_til, lysing=@lysing, timastimpill=@timastimpill, ar=@ar, vsid=@vsid, koid=@koid where id=@id"
                       , con))
                    {
                        con.Open();
                        cmd.Parameters.Add(new SqlParameter("id", item.Id));
                        cmd.Parameters.Add(new SqlParameter("dags_fra", item.DagsFra));
                        cmd.Parameters.Add(new SqlParameter("dags_til", item.DagsTil));

                        var lysingShort = String.Empty;
                        if (item.Lysing.Length > 25)
                        {
                            lysingShort = item.Lysing.Substring(0, 24) + "...";
                        }
                        else
                        {
                            lysingShort = item.Lysing.Substring(0, item.Lysing.Length - 1) + "...";
                        }

                        cmd.Parameters.Add(new SqlParameter("lysing", lysingShort));

                        //CultureInfo isl = new CultureInfo("is-IS");
                        //DateTime myDate = DateTime.Parse(DateTime.Now, isl.DateTimeFormat);
                        ////cmd.Parameters.Add(new SqlParameter("entryoccurred", driveEventItem.Entry.occurred));
                        //SqlParameter entryoccurredDateTimeParam = new SqlParameter("@entryoccurred", SqlDbType.DateTime);
                        //entryoccurredDateTimeParam.Value = myDate;
                        //cmd.Parameters.Add(entryoccurredDateTimeParam);
                        cmd.Parameters.Add(new SqlParameter("timastimpill", DateTime.Now));
                        cmd.Parameters.Add(new SqlParameter("Ar", item.DagsFra.Year.ToString()));
                        cmd.Parameters.Add(new SqlParameter("vsid", item.VsId));
                        cmd.Parameters.Add(new SqlParameter("koid", "-1")); // item.KoId));

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
            using (SqlConnection con = new SqlConnection(Constants.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT MAX(id) FROM veidiferdir", con))
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
            using (SqlConnection con = new SqlConnection(Constants.connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT count(*) FROM veidiferdir where id = @id", con))
                {
                    cmd.Parameters.Add(new SqlParameter("id", id));
                    //Associate connection with your command an open it
                    i = (int)cmd.ExecuteScalar();
                    if (i>0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Veidiferd GetVeidiferd(string id)
        {
            Logger.Logg("Starting, Get Repo - Veidiferd by id "+id+"...");
            string strQuery = @"Select id, vsid, lysing, dags_fra, dags_til, vet_id from veidiferdir where id = @id";

            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add(new SqlParameter("id", id));
            var connectionString = Constants.connectionString;
            //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
            DataTable dt = DatabaseService.GetData(cmd, connectionString);

            List<Veidiferd> list = new List<Veidiferd>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Veidiferd item = new Veidiferd();
                    item.Id = row["id"].ToString();
                    item.VsId = row["vsid"].ToString();
                    item.Lysing = row["lysing"].ToString();
                    item.DagsFra = GetDate(row["dags_fra"].ToString());
                    item.DagsTil = GetDate(row["dags_til"].ToString());
                    item.VetId = row["vet_id"].ToString();

                    Veididagatal_textiRepository repoText = new Veididagatal_textiRepository();
                    string text = string.Empty;
                    if (!string.IsNullOrWhiteSpace(row["vet_id"].ToString()))
                    { 
                        text = repoText.GetVeidiferdTexti(row["vet_id"].ToString());
                    }
                    item.LysingLong = text;

                    list.Add(item);
                }

            }
            return list[0];
        }


        private DateTime GetDate(string date)
        {
            DateTime goodDate;
            if( DateTime.TryParse(date, out goodDate))
            {
                return goodDate;
            }
            return new DateTime(2999,1,1);
        }

        public List<Veidiferd> GetVeidiferdir()
        {
            Logger.Logg("Starting, Get Repo - Veidiferdir...");
            string strQuery = @"Select id, vsid, lysing, dags_fra, dags_til from veidiferdir order by dags_fra asc";

            SqlCommand cmd = new SqlCommand(strQuery);

            var connectionString = Constants.connectionString;
            //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
            DataTable dt = DatabaseService.GetData(cmd, connectionString);

            List<Veidiferd> list = new List<Veidiferd>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Veidiferd item = new Veidiferd();
                    item.Id = row["id"].ToString();
                    item.VsId = row["vsid"].ToString();
                    item.Lysing = row["lysing"].ToString();
                    item.DagsFra = GetDate(row["dags_fra"].ToString());
                    item.DagsTil = GetDate(row["dags_til"].ToString());
                    list.Add(item);
                }

            }
            return list;
        }

    }
}
