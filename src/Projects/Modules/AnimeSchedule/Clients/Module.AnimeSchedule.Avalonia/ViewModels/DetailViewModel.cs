using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Avalonia.ViewModels
{
    public interface IChangeableViewModel
    {
        event Action OnChange;
    }
}
