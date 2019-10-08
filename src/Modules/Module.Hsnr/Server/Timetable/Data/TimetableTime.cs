namespace Module.Hsnr.Timetable.Data
{
    public class TimetableTime
    {
        public int Start { get; }
        
        public int End { get; }

        public TimetableTime(int end, int start)
        {
            this.End = end;
            this.Start = start;
        }
    }
}