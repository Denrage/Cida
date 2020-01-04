namespace Module.Hsnr.Timetable.Parser
{
    public interface IParser<in TIn, out TOut>
    {
        TOut Parse(TIn value);
    }
}