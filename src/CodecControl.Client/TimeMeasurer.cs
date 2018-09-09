using System;
using System.Diagnostics;
using System.Linq;
using NLog;
using NLog.Config;

namespace CodecControl.Client
{
    public class TimeMeasurer : IDisposable
    {
        private readonly LogLevel _level;
        private readonly Stopwatch _stopwatch;
        private readonly string _message;
        private readonly bool _isEnabled;

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public TimeMeasurer(string message, bool logStartMessage = false, LogLevel level = null)
        {
            _level = level ?? LogLevel.Trace;

            LoggingRule rule = LogManager.Configuration.LoggingRules.FirstOrDefault();
            _isEnabled = rule != null && rule.IsLoggingEnabledForLevel(_level);

            if (_isEnabled)
            {
                _message = message;

                if (logStartMessage)
                {
                    string s = $"Begin {_message}";
                    Log(s);
                }

                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
        }
         
        private void Log(string s)
        {
            log.Log(_level, s);
        }

        public TimeSpan ElapsedTime => _isEnabled ? _stopwatch.Elapsed : TimeSpan.Zero;

        public void Dispose()
        {
            if (_isEnabled)
            {
                _stopwatch.Stop();
                TimeSpan runTime = _stopwatch.Elapsed;

                string runTimeString = runTime.TotalSeconds > 1
                    ? $"{runTime.TotalSeconds} s"
                    : $"{runTime.TotalMilliseconds} ms";

                string formattedString = $"{_message} took {runTimeString}";
                Log(formattedString);
            }
        }
    }
}
