﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole
{
    public partial class FishingPlaceFishingLicenseSeller
    {
        public long Id { get; set; }
        public int? FishingPlaceId { get; set; }
        public int? FishingLicenceSellerId { get; set; }
    }
}
