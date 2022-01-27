using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Codepedia
{
    public class TaskManager<T>
    {
        readonly Func<CancellationToken, Task<T>> _initializer;

        public TaskManager(Func<CancellationToken, Task<T>> initializer) => _initializer = initializer;

        public TaskManager(Func<Task<T>> initializer) : this(_ => initializer()) { }

        Task<T>? _task;
        CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        readonly object _initializationLock = new();
        readonly ConcurrentBag<CancellationToken> _cancellationTokens = new();

        public Task<T> Value (CancellationToken cancellationToken)
        {
            if (cancellationToken != default)
            {
                if (cancellationToken.IsCancellationRequested)
                    _cancellationTokens.Add(cancellationToken);
                cancellationToken.Register(() =>
                {
                    if (_task is not { IsCompleted: true } && _cancellationTokens.All(c => c.IsCancellationRequested))
                        _cancellationToken.Cancel();
                });
            }

            if (_task != null && !_cancellationToken.IsCancellationRequested) return _task;
            lock (_initializationLock)
            {
                if (_task != null) return _task;

                _task = _initializer(_cancellationToken.Token);
            }
            return _task;
        }
    }
}
