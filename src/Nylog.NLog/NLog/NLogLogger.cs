using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using INLogLogger = global::NLog.ILogger;
using NLogLogLevel = global::NLog.LogLevel;

namespace Nylog.NLog
{
    public class NLogLogger : ILogger
    {
        private readonly INLogLogger _logger;

        public NLogLogger(INLogLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;

        }
        
        public void Log(LogLevel level, IDictionary<string, object> state, Exception exception)
        {
            var nlogLogLevel = GetLogLevel(level);

            string message = null;

            if (state.ContainsKey("message"))
            {
                message = (string)state["message"];
            }

            var eventInfo = CreateLogEventInfo(nlogLogLevel, message, state, exception);

            _logger.Log(eventInfo);

        }

        private LogEventInfo CreateLogEventInfo(NLogLogLevel level, string message, IDictionary<string, object> dictionary, Exception exception)
        {
            LogEventInfo logEvent = new LogEventInfo(level, _logger.Name, message);

            foreach (var item in dictionary)
            {
                logEvent.Properties[item.Key] = item.Value;
            }

            if (exception != null)
            {
                logEvent.Properties["error-source"] = exception.Source;
                logEvent.Properties["error-class"] = exception.TargetSite.DeclaringType.FullName;
                logEvent.Properties["error-method"] = exception.TargetSite.Name;
                logEvent.Properties["error-message"] = exception.Message;

                if (exception.InnerException != null)
                {
                    logEvent.Properties["inner-error-message"] = exception.InnerException.Message;
                }
            }

            return logEvent;
        }

        public bool IsEnabled(LogLevel level)
        {
            return _logger.IsEnabled(GetLogLevel(level));
        }

        private NLogLogLevel GetLogLevel(LogLevel level)
        {
            NLogLogLevel result;

            if (!LogLevels.TryGetValue(level, out result))
            {
                result = NLogLogLevel.Debug;
            }

            return result;
        }

        private static readonly IReadOnlyDictionary<LogLevel, NLogLogLevel> LogLevels = new Dictionary<LogLevel, NLogLogLevel>
        {
            [LogLevel.Verbose] = NLogLogLevel.Trace,
            [LogLevel.Debug] = NLogLogLevel.Debug,
            [LogLevel.Information] = NLogLogLevel.Info,
            [LogLevel.Warning] = NLogLogLevel.Warn,
            [LogLevel.Error] = NLogLogLevel.Error,
            [LogLevel.Critical] = NLogLogLevel.Fatal
        };
    }
}
