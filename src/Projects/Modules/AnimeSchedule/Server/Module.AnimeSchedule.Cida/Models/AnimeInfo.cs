using System.ComponentModel.DataAnnotations.Schema;

namespace Module.AnimeSchedule.Cida.Models;

public class AnimeInfo
{
    public uint Id { get; set; }
    
    public string Identifier { get; set; }
    
    [ForeignKey(nameof(Schedule))]
    public uint ScheduleId { get; set; }
    
    public AnimeInfoType Type { get; set; }
    
    public List<Episode> Episodes { get; set; }
    
    public AnimeFolder AnimeFolder { get; set; }
    
    public AnimeFilter AnimeFilter { get; set; }
    
    public Schedule Schedule {  get; set; }
}
