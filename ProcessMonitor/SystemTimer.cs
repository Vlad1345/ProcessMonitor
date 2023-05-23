using System; 
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.ComponentModel;

namespace ProcessMonitor
{
    //Timer functionality
    public interface ITimer
    {
        void Start();
        bool StartWithReset();
        void Stop();
        void Close();
        void Dispose();
        bool AutoReset { get; set; }
        bool Enabled { get; set; }
        double Interval { get; set; }
        ISynchronizeInvoke SynchronizingObject { get; set; }

        event ElapsedEventHandler Elapsed;
    }
    class SystemTimer : Timer, ITimer
    {
        public SystemTimer() : base() { }
        public SystemTimer(double interval)
        : this()
        {
            this.Interval = interval;
        }
        public bool StartWithReset()
        {
            this.AutoReset = true;
            this.Enabled = true;

            return true;
        }

    }
}
