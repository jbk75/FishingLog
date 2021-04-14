using Microsoft.EntityFrameworkCore;

namespace VedurConsole
{
    public class WeatherContext : DbContext
    {
        private readonly string _connectionString;

        public WeatherContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        ///public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Weather> Weather { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=tcp:aflaklo.database.windows.net,1433;Initial Catalog=Aflaklo;Persist Security Info=False;User ID=aflaklo;Password=Afafaf93;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
