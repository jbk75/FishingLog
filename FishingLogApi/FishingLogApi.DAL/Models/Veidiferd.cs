namespace FishingLogApi.DAL.Models;

public class Veidiferd
{
    public int? Id { get; set; }
    public string? Description { get; set; }
    public DateTime Timastimpill { get; set; } = DateTime.Now;
    public DateTime DagsFra { get; set; }
    public DateTime DagsTil { get; set; }
    public int VsId { get; set; }
    //public int VetId { get; set; }
    //public int KoId { get; set; }
}
