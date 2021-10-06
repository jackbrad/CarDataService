using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace CarDataService
{
    public class Log
    {
        private readonly TelemetryClient _tc;
        internal Log(TelemetryClient tc)
        {
            _tc = tc;
        }

        public void Flush()
        {
            _tc.Flush();
        }

        public void Debug(string message, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Verbose);
        }

        public void Debug(string message, IDictionary<string, string> props, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Verbose, props);
        }

        public void Error(Exception ex, [CallerMemberName] string caller = "")
        {
            _tc.TrackException(ex);
            //_tc.TrackTrace(ex.Message, SeverityLevel.Critical);
        }

        public void Error(string message, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Error);
        }

        public void Event(string name, IDictionary<string, string> props = null, IDictionary<string, double> metric = null)
        {
            _tc.TrackEvent(name, props, metric);
        }

        public void Request(string name, DateTimeOffset startTime, TimeSpan duration)
        {
            _tc.TrackRequest(name, startTime, duration, "OK", true);
        }

        public void FailedRequest(string name, DateTimeOffset startTime, TimeSpan duration)
        {
            _tc.TrackRequest(name, startTime, duration, "OK", false);
        }

        public void Info(string message, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Information);
        }

        public void Warn(string message, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Warning);
        }

        public void Critical(string message, [CallerMemberName] string caller = "")
        {
            _tc.TrackTrace($"{caller}: {message}", SeverityLevel.Critical);
        }
    }
}