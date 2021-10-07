using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.AnimeSchedule.Avalonia.ViewModels;

public abstract class AssignmentViewModel<T1, T2, Comparer> : ViewModelBase
    where T1 : class
    where T2 : class
    where Comparer : IEqualityComparer<T2>, new()
{
    private readonly List<T2> allAssignableItems = new();

    private T1 item;

    public T1 Item
    {
        get => this.item;
        set
        {
            if (this.item != value)
            {
                this.item = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Assigned));
                this.RaisePropertyChanged(nameof(this.NotAssigned));
            }
        }
    }

    public AvaloniaList<T2> Assigned => new(this.GetAssignedItems());

    public AvaloniaList<T2> NotAssigned => new(this.allAssignableItems.Where(x => !this.Assigned.Contains(x, new Comparer())));

    public AssignmentViewModel(T1 item)
    {
        this.item = item;
        Task.Run(async () =>
        {
            this.allAssignableItems.Clear();
            this.allAssignableItems.AddRange(await this.GetAssignableItems());
            this.RaisePropertyChanged(nameof(this.NotAssigned));
        });
    }

    protected abstract Task<IEnumerable<T2>> GetAssignableItems();

    protected abstract IEnumerable<T2> GetAssignedItems();

    protected abstract Task<bool> AssignInternal(T2 item);

    protected abstract void Add(T2 item);

    protected abstract Task<bool> UnassignInternal(T2 item);

    protected abstract void Remove(T2 item);

    public async Task Assign(T2 item)
    {
        var result = await this.AssignInternal(item);

        if (result)
        {
            this.Add(item);
            this.RaisePropertyChanged(nameof(this.Assigned));
            this.RaisePropertyChanged(nameof(this.NotAssigned));
        }
    }

    public async Task Unassign(T2 item)
    {
        var result = await this.UnassignInternal(item);

        if (result)
        {
            this.Remove(item);
            this.RaisePropertyChanged(nameof(this.Assigned));
            this.RaisePropertyChanged(nameof(this.NotAssigned));
        }
    }
}
