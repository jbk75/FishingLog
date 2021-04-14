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
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
