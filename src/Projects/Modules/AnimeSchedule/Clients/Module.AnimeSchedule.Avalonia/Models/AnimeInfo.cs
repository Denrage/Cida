namespace Module.AnimeSchedule.Avalonia.Models;

public class AnimeInfo
{
    public int Id { get; set; }

    public string Identifier { get; set; } = string.Empty;
    
    public AnimeType Type { get; set; }

    public string AnimeFolder { get; set; } = string.Empty;

    public string Filter {  get; set; } = string.Empty;
}
