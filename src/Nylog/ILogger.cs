using System;
using System.Collections.Generic;

namespace Nylog
{
    public interface ILogger
    {
        void Log(LogLevel level, IDictionary<string, object> state, Exception exception);

        bool IsEnabled(LogLevel level);
    }
}