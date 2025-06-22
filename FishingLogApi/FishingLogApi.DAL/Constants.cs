
namespace FishingLogApi.DAL
{
    public static class Constants
    {
//    public static string connectionString = @"Data Source=LAP-JOI\SQLEXPRESS;Initial Catalog=fishinglog;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        //public static string connectionString = @"Data Source=LAP-JOI\SQLEXPRESS;Initial Catalog=fishinglog;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static string connectionString 
            = @"Data Source=localhost,1433;Initial Catalog=fishinglogg;User ID=SA;Password=YourStrong!Passw0rd;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

    }
}
