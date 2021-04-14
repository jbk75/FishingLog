using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole
{
    public partial class FishingStatisticsPerYear
    {
        public long Id { get; set; }
        public int? FishingPlaceId { get; set; }
        public int? TotalSalmon { get; set; }
        public int? TotalTrout { get; set; }
        public int? TotalRods { get; set; }
        public string Year { get; set; }
        public DateTime? DateOfStatistics { get; set; }
    }
}
