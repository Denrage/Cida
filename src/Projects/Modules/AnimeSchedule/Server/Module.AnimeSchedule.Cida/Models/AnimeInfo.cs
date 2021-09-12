namespace Module.AnimeSchedule.Cida.Models;

public class AnimeInfo
{
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    
    public string Identifier { get; set; }
    
    public int ScheduleId { get; set; }
    
    public AnimeInfoType Type { get; set; }
    
    public List<Episode> Episodes { get; set; }
    
    public AnimeFolder AnimeFolder { get; set; }
    
    public AnimeFilter AnimeFilter { get; set; }
    
    public List<Schedule> Schedules {  get; set; }
}
