using System;
using System.Collections.Generic;

#nullable disable

namespace VedurConsole
{
    public partial class Weather
    {
        public long Id { get; set; }
        public DateTime? Dags { get; set; }
        public string Breidd { get; set; }
        public string Daggarmark { get; set; }
        public string Haed { get; set; }
        public double? Hiti { get; set; }
        public string Lengd { get; set; }
        public string Loftthrystingur { get; set; }
        public string Nafn { get; set; }
        public int Nr { get; set; }
        public string Nr_Vedurstofa { get; set; }
        public string PntX { get; set; }
        public string PntY { get; set; }
        public string Raki { get; set; }
        public string Sjavarhaed { get; set; }
        public string Vindatt { get; set; }
        public string VindattAsc { get; set; }
        public string VindattStDev { get; set; }
        public string Vindhradi { get; set; }
        public string Vindhvida { get; set; }
    }
}
