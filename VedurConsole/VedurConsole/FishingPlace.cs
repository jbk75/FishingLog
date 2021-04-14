using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole
{
    public partial class FishingPlace
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public int? NumberOfSpots { get; set; }
        public string Description { get; set; }
        public int? FishingPlaceTypeId { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime? PrimeTimeFromDate { get; set; }
        public DateTime? PrimeTimeToDate { get; set; }
    }
}
