namespace Module.AnimeSchedule.Avalonia.Models;

public class AnimeInfo
{
    public int Id { get; set; }

    public string Identifier { get; set; }
    
    public AnimeType Type { get; set; }

    public string AnimeFolder { get; set; }

    public string Filter {  get; set; }
}
