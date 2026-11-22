using System;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RevitNumbering.Revit
{
    internal sealed class RevitEventExecutor : IExternalEventHandler
    {
        private readonly ExternalEvent _externalEvent;
        private Action<UIApplication> _action;

        public RevitEventExecutor()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public Task<T> InvokeAsync<T>(Func<UIApplication, T> action)
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

            return completion.Task;
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
