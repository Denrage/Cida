namespace Module.Hsnr.Timetable.Data
{
    public class Subject
    {
        public int Start { get; set; }

        public int End { get; set; }

        public string Lecturer { get; set; }

        public string Room { get; set; }

        public string Name { get; set; }

        public override string ToString() =>
            $"Start: {this.Start.ToString()} End: {this.End.ToString()} Lecturer: {this.Lecturer} Room: {this.Room} Name: {this.Name}";
    }
}