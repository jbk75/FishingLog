using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole
{
    public partial class VTrip
    {
        public int Id { get; set; }
        public int FishingPlaceId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Description { get; set; }
        public DateTime? DateCreated { get; set; }
        public string Name { get; set; }
    }
}
