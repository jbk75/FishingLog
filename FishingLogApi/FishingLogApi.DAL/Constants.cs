using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingLogApi.DAL
{
    public static class Constants
    {
//    public static string connectionString = @"Data Source=LAP-JOI\SQLEXPRESS;Initial Catalog=fishinglog;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //public static string connectionString = @"Data Source=LAP-JOI\SQLEXPRESS;Initial Catalog=fishinglog;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static string connectionString = @"Server=tcp:fishinglog.database.windows.net,1433;Initial Catalog=FishingLogg;User ID=adminfishing;Password=Fififi75;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;";

    }
}
