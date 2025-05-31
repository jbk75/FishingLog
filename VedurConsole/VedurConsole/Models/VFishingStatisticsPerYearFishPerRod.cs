using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole.Models
{
    public partial class VFishingStatisticsPerYearFishPerRod
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public int? FishingPlaceId { get; set; }
        public int? TotalSalmon { get; set; }
        public int? TotalTrout { get; set; }
        public int? TotalRods { get; set; }
        public string Year { get; set; }
        public DateTime? DateOfStatistics { get; set; }
        public int? SalmonPerRod { get; set; }
        public int? SalmonPerRodPerDay { get; set; }
    }
}
