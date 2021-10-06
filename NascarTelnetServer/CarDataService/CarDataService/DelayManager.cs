using System;
using System.Collections.Generic;


namespace CarDataService
{
    internal class Delay
    {
        public TimeSpan DelayTime { get; set; }
        public int Multiplier { get; set; }
        public TimeSpan ResetTime { get; set; }
        public int TerminationThreshold { get; set; }
    }

    internal class DelayManager
    {
        private readonly Log _logger;

        private readonly Dictionary<DelayType, Delay> _delays = new Dictionary<DelayType, Delay>();

        private readonly Dictionary<DelayType, Tuple<int, DateTime>> _events = new Dictionary<DelayType, Tuple<int, DateTime>>();

        internal DelayManager(Log logger)
        {
            _logger = logger;
            SetupDefaultDelays();
        }

        internal DelayManager(Log logger, Dictionary<DelayType, Delay> delays) : this(logger)
        {
            _delays = delays;
        }

        private void SetupDefaultDelays()
        {
            var exception = new Delay()
            {
                DelayTime = new TimeSpan(0, 0, 0, 1),
                Multiplier = 2,
                ResetTime = new TimeSpan(0, 1, 0, 0),
                TerminationThreshold = 5
            };

            var empty = new Delay()
            {
                DelayTime = new TimeSpan(0, 0, 0, 1),
                Multiplier = 2,
                ResetTime = new TimeSpan(0, 1, 0, 0),
                TerminationThreshold = 10
            };
            _delays.Add(DelayType.Exception, exception);
            _delays.Add(DelayType.EmptyQueue, empty);
        }

        internal int GetNextDelayMilliseconds(DelayType delayType)
        {
            var delay = _delays[delayType];
            _logger.Debug(string.Format("Checking delay time for {0}", delayType));
            var defaultWaitTime = delay.DelayTime.TotalMilliseconds;
            _logger.Debug(string.Format("Default delay time {0}", defaultWaitTime));

            if (_events.ContainsKey(delayType))
            {
                var last = _events[delayType];
                _logger.Debug(string.Format("Existing delays exist; {0}: {1} hit(s), Last hit {2}", delayType, last.Item1, last.Item2));
                var resetTimespan = delay.ResetTime;
                var shouldMultiply = last.Item2.Add(resetTimespan) < DateTime.UtcNow;

                if (shouldMultiply)
                {
                    _logger.Debug(string.Format("Previous events found within the last: {0} milliseconds, multiplying ",
                        resetTimespan.TotalMilliseconds));
                    _events[delayType] = new Tuple<int, DateTime>(last.Item1 + 1, DateTime.UtcNow);
                    var finalDelay = (int)defaultWaitTime * (delay.Multiplier * last.Item1);
                    _logger.Debug(string.Format("Calculated delay for {0}: {1} milliseconds", delayType, finalDelay));
                    return finalDelay;
                }
                _logger.Debug(string.Format("Previous {0} events out of time range {1}; default delay {2} milliseconds",
                    last.Item1, resetTimespan, defaultWaitTime));
            }

            _events[delayType] = new Tuple<int, DateTime>(1, DateTime.UtcNow);
            _logger.Debug(string.Format("No existing delays for {0}, returning default delay of {1} milliseconds", delayType, defaultWaitTime));
            return (int)defaultWaitTime;
        }

        internal bool ShouldTerminate(DelayType delay)
        {
            var delaySettings = _delays[delay];
            if (!_events.ContainsKey(delay)) return false;
            var last = _events[delay];
            var shouldMultiply = last.Item2.Add(delaySettings.ResetTime) < DateTime.UtcNow;

            if (!shouldMultiply) return false;
            var shouldTerminate = last.Item1 > delaySettings.TerminationThreshold;
            if (!shouldTerminate) return false;
            _events.Remove(delay);
            return true;
        }
    }

    public enum DelayType
    {
        Exception,
        EmptyQueue
    }
}
