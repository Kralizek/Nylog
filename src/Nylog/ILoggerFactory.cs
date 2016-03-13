using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nylog
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string categoryName);

        void AddProvider(ILoggerProvider provider);

        IReadOnlyList<ILoggerProvider> GetProviders();

        LogLevel MinimumLevel { get; set; }
    }
}
