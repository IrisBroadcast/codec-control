#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

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

            LoggingRule rule = LogManager.Configuration?.LoggingRules.FirstOrDefault();
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
