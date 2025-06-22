

using Microsoft.Data.SqlClient;
using System.Data;

namespace FishingLogApi.DAL.Repositories;

public class VeidistadurRepository
{
    public List<Veidistadur> GetVeidistadir()
    {
        //Logger.Logg("Starting, Get Repo - Veidistadir...");
        string strQuery = @"Select id, name from fishingplace order by name asc";

        SqlCommand cmd = new SqlCommand(strQuery);

        var connectionString = Constants.connectionString;
        //Logger.Logg("Starting, GetCompany..., GetData, ConnectionString=" + connectionString);
        DataTable dt = DatabaseService.GetData(cmd, connectionString);

        List<Veidistadur> vst = new List<Veidistadur>();
        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                Veidistadur item = new Veidistadur();
                item.Heiti = row["name"].ToString();
                item.VsId = Convert.ToInt32(row["id"].ToString());
                vst.Add(item);
            }

        }
        return vst;
    }
}
