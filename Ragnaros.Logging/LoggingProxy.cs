using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Ragnaros.Logging
{
    public class LoggingProxy<T> : DispatchProxy
    {
        private T _decorated;
        private ILogger _logger;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                LogBefore(targetMethod, args);

                var result = targetMethod.Invoke(_decorated, args);

                LogAfter(targetMethod, args, result);

                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                LogException(ex.InnerException ?? ex, targetMethod);
                throw ex.InnerException ?? ex;
            }
        }

        public static T Create(T decorated, ILogger logger)
        {
            object proxy = Create<T, LoggingProxy<T>>();
            ((LoggingProxy<T>)proxy).SetParameters(decorated, logger);

            return (T)proxy;
        }

        private void SetParameters(T decorated, ILogger logger)
        {
            if (decorated == null) throw new ArgumentNullException(nameof(decorated));
            _decorated = decorated;

            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
        }

        private void LogException(Exception exception, MethodInfo methodInfo = null)
        {
            _logger.LogError("Class {@class}, Method {@method} threw exception:\n {@exception}", _decorated.GetType().FullName, methodInfo.Name, exception);
        }

        private void LogAfter(MethodInfo methodInfo, object[] args, object result)
        {
            _logger.LogInformation("Class {@class}, Method {@method} executed, Output: {@output}", _decorated.GetType().FullName, methodInfo.Name, result);
        }

        private void LogBefore(MethodInfo methodInfo, object[] args)
        {
            _logger.LogInformation("Class {@class}, Method {@method} is executing. Input: {@input}", _decorated.GetType().FullName, methodInfo.Name, args);
        }
    }
}
