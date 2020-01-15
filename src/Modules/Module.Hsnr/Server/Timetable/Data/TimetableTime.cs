namespace Module.Hsnr.Timetable.Data
{
    public class TimetableTime
    {
        public int Start { get; }
        
        public int End { get; }

        public TimetableTime(int start, int end)
        {
            this.End = end;
            this.Start = start;
        }
    }
}