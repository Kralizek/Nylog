using System;

namespace Nylog
{
    public interface ILoggerProvider : IDisposable
    {
        ILogger CreateLogger(string name);
    }
}