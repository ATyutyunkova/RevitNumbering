using System;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RevitNumbering.Revit
{
    /// <summary>
    /// Выполняет действие в контексте Revit API через ExternalEvent.
    /// Один вызов за раз; таймаут возвращает ошибку, но уже поставленное действие не отменяет.
    /// </summary>
    internal sealed class RevitEventExecutor : IExternalEventHandler
    {
        private readonly ExternalEvent _externalEvent;
        private Action<UIApplication> _action;

        public RevitEventExecutor()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public async Task<T> InvokeAsync<T>(
            Func<UIApplication, T> action,
            TimeSpan? timeout = null)
        {
            var completion = new TaskCompletionSource<T>();

            _action = application =>
            {
                try
                {
                    completion.TrySetResult(action(application));
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            };

            var request = _externalEvent.Raise();
            if (request != ExternalEventRequest.Accepted)
            {
                _action = null;
                completion.TrySetException(new InvalidOperationException("Revit занят, повторите действие."));
            }

            if (timeout.HasValue)
            {
                var finished = await Task.WhenAny(completion.Task, Task.Delay(timeout.Value));
                if (finished != completion.Task)
                {
                    Logger.Error("Revit не ответил за " + timeout.Value.TotalSeconds + " с.");
                    completion.TrySetException(
                        new TimeoutException("Revit не ответил вовремя. Попробуйте «Обновить»."));
                }
            }

            return await completion.Task;
        }

        public void Execute(UIApplication application)
        {
            var action = Interlocked.Exchange(ref _action, null);
            action?.Invoke(application);
        }

        public string GetName()
        {
            return "RevitNumbering";
        }
    }
}
