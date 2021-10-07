using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Avalonia.ViewModels
{
    public abstract class EditViewModel<T> : ViewModelBase
        where T: class
    {
        private readonly T item;

        public T Item => this.item;

        public EditViewModel(T item)
        {
            this.item = item;
        }
    }
}
