using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole.Models
{
    public partial class FishingNews
    {
        public long Id { get; set; }
        public int? FishingPlaceId { get; set; }
        public DateTime? DateOfNews { get; set; }
        public string NewsText { get; set; }
    }
}
