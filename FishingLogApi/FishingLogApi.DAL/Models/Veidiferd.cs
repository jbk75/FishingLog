using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingLogApi.DAL.Models
{
    public class Veidiferd
    {
        public string Id { get; set; }
        public string Lysing { get; set; }

        public string LysingLong { get; set; }
        public DateTime Timastimpill { get; set; }
        public DateTime DagsFra { get; set; }
        public DateTime DagsTil { get; set; }
        public string VsId { get; set; }
        public string VetId { get; set; }
        public string KoId { get; set; }
    }
}
