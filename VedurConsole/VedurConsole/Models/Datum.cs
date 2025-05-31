using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole.Models
{
    public partial class Datum
    {
        public int Id { get; set; }
        public DateTime DagsFra { get; set; }
        public DateTime DagsTil { get; set; }
        public string Lysing { get; set; }
        public DateTime Timastimpill { get; set; }
        public int Ar { get; set; }
        public int VetId { get; set; }
        public int? Vsid { get; set; }
        public string Koid { get; set; }
    }
}
