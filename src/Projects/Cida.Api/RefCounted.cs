using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cida.Api
{
    public class RefCounted<T>
        where T : IDisposable
    {
        private int count;
        private T value;
        private readonly Func<Task<T>> factory;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public RefCounted(Func<Task<T>> factory)
        {
            this.factory = factory;
        }

        public async Task<T> Acquire(CancellationToken cancellationToken = default)
        {
            await this.semaphore.WaitAsync(cancellationToken);            
            try
            {
                if (count++ == 0)
                    this.value = await factory.Invoke();

                return this.value;
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        public async Task Release(CancellationToken cancellationToken = default)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            try
            {
                if (--count == 0)
                    this.value.Dispose();
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}
