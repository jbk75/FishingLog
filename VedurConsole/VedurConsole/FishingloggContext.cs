using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace VedurConsole
{
    public partial class FishingloggContext : DbContext
    {
        public FishingloggContext()
        {
        }

        public FishingloggContext(DbContextOptions<FishingloggContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Data2> Data2s { get; set; }
        public virtual DbSet<Datum> Data { get; set; }
        public virtual DbSet<FishingLicenseSeller> FishingLicenseSellers { get; set; }
        public virtual DbSet<FishingNews> FishingNews { get; set; }
        public virtual DbSet<FishingPlace> FishingPlaces { get; set; }
        public virtual DbSet<FishingPlaceFishingLicenseSeller> FishingPlaceFishingLicenseSellers { get; set; }
        public virtual DbSet<FishingPlaceType> FishingPlaceTypes { get; set; }
        public virtual DbSet<FishingStatisticsPerYear> FishingStatisticsPerYears { get; set; }
        public virtual DbSet<Trip> Trips { get; set; }
        public virtual DbSet<VFishingStatisticsPerYearFishPerRod> VFishingStatisticsPerYearFishPerRods { get; set; }
        public virtual DbSet<VTrip> VTrips { get; set; }
        public virtual DbSet<Weather> Weathers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=tcp:fishinglog.database.windows.net,1433;Initial Catalog=Fishinglogg;Persist Security Info=False;User ID=Adminfishing;Password=Fififi75;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Data2>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("DATA2");

                entity.Property(e => e.VetId).HasColumnName("vet_id");

                entity.Property(e => e.VetTexti)
                    .IsRequired()
                    .HasColumnName("vet_texti");
            });

            modelBuilder.Entity<Datum>(entity =>
            {
                entity.ToTable("data");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.DagsFra)
                    .HasColumnType("datetime")
                    .HasColumnName("Dags_fra");

                entity.Property(e => e.DagsTil)
                    .HasColumnType("datetime")
                    .HasColumnName("Dags_til");

                entity.Property(e => e.Koid)
                    .HasMaxLength(50)
                    .HasColumnName("koid");

                entity.Property(e => e.Lysing)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Timastimpill).HasColumnType("datetime");

                entity.Property(e => e.VetId).HasColumnName("vet_id");

                entity.Property(e => e.Vsid).HasColumnName("vsid");
            });

            modelBuilder.Entity<FishingLicenseSeller>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("FishingLicenseSeller");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.WebPage).HasMaxLength(200);
            });

            modelBuilder.Entity<FishingNews>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.DateOfNews).HasColumnType("datetime");

                entity.Property(e => e.FishingPlaceId).HasColumnName("FishingPlaceID");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");

                entity.Property(e => e.NewsText).HasMaxLength(500);
            });

            modelBuilder.Entity<FishingPlace>(entity =>
            {
                entity.ToTable("FishingPlace");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CloseDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.FishingPlaceTypeId).HasColumnName("FishingPlaceTypeID");

                entity.Property(e => e.Latitude).HasMaxLength(50);

                entity.Property(e => e.Longitude).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.OpenDate).HasColumnType("datetime");

                entity.Property(e => e.PrimeTimeFromDate).HasColumnType("datetime");

                entity.Property(e => e.PrimeTimeToDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<FishingPlaceFishingLicenseSeller>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("FishingPlaceFishingLicenseSeller");

                entity.Property(e => e.FishingLicenceSellerId).HasColumnName("FishingLicenceSellerID");

                entity.Property(e => e.FishingPlaceId).HasColumnName("FishingPlaceID");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");
            });

            modelBuilder.Entity<FishingPlaceType>(entity =>
            {
                entity.ToTable("FishingPlaceType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FishingStatisticsPerYear>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("FishingStatisticsPerYear");

                entity.Property(e => e.DateOfStatistics).HasColumnType("datetime");

                entity.Property(e => e.FishingPlaceId).HasColumnName("FishingPlaceID");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");

                entity.Property(e => e.Year)
                    .HasMaxLength(4)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.ToTable("Trip");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateFrom).HasColumnType("datetime");

                entity.Property(e => e.DateTo).HasColumnType("datetime");
            });

            modelBuilder.Entity<VFishingStatisticsPerYearFishPerRod>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("v_fishingStatisticsPerYear_FishPerRod");

                entity.Property(e => e.DateOfStatistics).HasColumnType("datetime");

                entity.Property(e => e.FishingPlaceId).HasColumnName("FishingPlaceID");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("name");

                entity.Property(e => e.Year)
                    .HasMaxLength(4)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<VTrip>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("v_Trips");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateFrom).HasColumnType("datetime");

                entity.Property(e => e.DateTo).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Weather>(entity =>
            {
                entity.ToTable("Weather");

                entity.Property(e => e.Id).HasColumnName("WeatherID");

                entity.Property(e => e.Nr_Vedurstofa).HasColumnName("Nr_Vedurstofa");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
