using Module.AnimeSchedule.Cida.Models;

namespace Module.AnimeSchedule.Cida.Interfaces;

public interface ITestable : IActionable
{
    AnimeTestResult GetTestResult();
}
