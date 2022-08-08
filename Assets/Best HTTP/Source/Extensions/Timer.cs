using System;
using System.Collections.Generic;

namespace BestHTTP.Extensions
{
    public
#if CSHARP_7_OR_LATER
        readonly
#endif
            struct TimerData
    {
        public readonly DateTime Created;
        public readonly TimeSpan Interval;
        public readonly object Context;

        public readonly Func<object, bool> OnTimer;

        public bool IsOnTime { get { return DateTime.Now >= this.Created + this.Interval; } }

        public TimerData(TimeSpan interval, object context, Func<object, bool> onTimer)
        {
            this.Created = DateTime.Now;
            this.Interval = interval;
            this.Context = context;
            this.OnTimer = onTimer;
        }

        /// <summary>
        /// Create a new TimerData but the Created field will be set to the current time.
        /// </summary>
        public TimerData CreateNew()
        {
            return new TimerData(this.Interval, this.Context, this.OnTimer);
        }

        public override string ToString()
        {
            return $"[TimerData Created: {this.Created}, Interval: {this.Interval}, IsOnTime: {this.IsOnTime}]";
        }
    }

    public static class Timer
    {
        private static List<TimerData> Timers = new List<TimerData>();

        public static void Add(TimerData timer)
        {
            Timers.Add(timer);
        }

        internal static void Process()
        {
            for (int i = 0; i < Timers.Count; ++i)
            {
                TimerData timer = Timers[i];

                if (timer.IsOnTime)
                {
                    bool repeat = timer.OnTimer(timer.Context);

                    if (repeat)
                        Timers[i] = timer.CreateNew();
                    else
                        Timers.RemoveAt(i--);
                }
            }
        }
    }
}
