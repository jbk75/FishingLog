using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using System;

namespace VedurConsole.Models
{
    [XmlRoot("ArrayOfVedur", Namespace = "http://schemas.datacontract.org/2004/07/VgApi.Models")]
    public class WeatherStationList
    {
        [XmlElement("Vedur", Namespace = "http://schemas.datacontract.org/2004/07/VgApi.Models")]
        public List<WeatherStationData> Items { get; set; }
    }

    public class WeatherStationData
    {
        public string Breidd { get; set; }
        public string Daggarmark { get; set; }
        public string Dags { get; set; }

        [XmlIgnore]
        public DateTime? ParsedDags => ParseDateTime(Dags);

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

        private static DateTime? ParseDateTime(string raw)
        {
            if (DateTime.TryParseExact(raw, "d.M.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return null;
        }
    }
}