using System; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace ProcessMonitor
{
    public class Monitor
    {
        public const char STOP_KEY = 'q';
        private StreamWriter sw;
        //processes that monitor keeps track of
        private Dictionary<int, int> monitoredProcesses;
        //found processes which will be compared to dictionary
        private List<ProcessInfo> foundProcesses;
        private int threshold, frequency;
        private string processName;
        private IProcessService service;
        private IHandler handler;
        private ITimer aTimer;




        public Monitor(string processName, int threshold, int frequency, IProcessService service, IHandler handler, ITimer aTimer)
        {
            sw = File.CreateText(System.Environment.CurrentDirectory + @"\Log.txt");
            this.frequency = frequency;
            this.threshold = threshold;
            this.processName = processName;
            monitoredProcesses = new Dictionary<int, int>();
            this.service = service;
            this.handler = handler;
            this.aTimer = aTimer;


        }
        public Dictionary<int, int> GetMonitoredProcesses()
        {
            return new Dictionary<int, int>(monitoredProcesses);
        }
        public char GetStopKey()
        {
            return STOP_KEY;
        }
        //starts monitor
        public void Start()
        {
            sw.WriteLine("Starting monitor");
            //there will be one check for processes before timer starts
            CheckForProcesses();
            //start timer - the next check will be every x minutes 
            SetTimer();

        }
        //gets running processes and updates the monitored ones
        private void CheckForProcesses()
        {
            sw.WriteLine("Checking for processes");

            foundProcesses = service.getProcesses(processName);

            Dictionary<int, int> updated = UpdateProcesses(monitoredProcesses, foundProcesses, sw);
            monitoredProcesses = updated;

        }
        private void SetTimer()
        {
            //Assign method to event handler
            aTimer.Elapsed += OnElapsed;
            //start timer with AutoReset set to true
            aTimer.StartWithReset();
        }
        private void OnElapsed(Object source, ElapsedEventArgs e)
        {
            CheckForProcesses();
        }
        //stops monitor
        public void Stop()
        {
            sw.WriteLine("Ending monitor");
            sw.Dispose();
            aTimer.Stop();
            aTimer.Dispose();
        }
        //logic for updating monitored dictionary
        public Dictionary<int, int> UpdateProcesses(Dictionary<int, int> monitoredProcesses, List<ProcessInfo> foundProcesses)
        {

            return UpdateProcesses(monitoredProcesses, foundProcesses, null);
        }
        public Dictionary<int, int> UpdateProcesses(Dictionary<int, int> monitoredProcesses, List<ProcessInfo> foundProcesses, StreamWriter sw)
        {
            try
            {
                //remove closed processes from monitor
                if (monitoredProcesses.Keys.Count > 0)
                {
                    List<int> list = foundProcesses.Select(o => o.getId()).ToList();
                    if (sw != null)
                    {
                        monitoredProcesses.Keys.Except(list).ToList().ForEach(key => sw.WriteLine("Process " + key + " no longer running"));
                    }
                    monitoredProcesses.Keys.Except(list).ToList().ForEach(key => monitoredProcesses.Remove(key));
                }

                //update timer of existing processes and add new ones
                foreach (ProcessInfo p in foundProcesses)
                {
                    if (monitoredProcesses.ContainsKey(p.getId()))
                    {
                        monitoredProcesses[p.getId()] += frequency;
                    }
                    else
                    {
                        monitoredProcesses.Add(p.getId(), 0);
                        if (sw != null)
                        {
                            sw.WriteLine("Process " + p.getId() + " added to monitor");
                        }
                    }

                    //remove process if it reached maximum lifetime
                    if (monitoredProcesses[p.getId()] >= threshold)
                    {

                        bool isProcessRemoved = handler.KillProcess(p.getId(), monitoredProcesses);
                        if (isProcessRemoved)
                        {
                            monitoredProcesses.Remove(p.getId());
                        }
                        if (sw != null)
                        {
                            if (isProcessRemoved)
                            {
                                sw.WriteLine("Process " + p.getId() + " exceeded threshold");
                            }
                            else
                            {
                                sw.WriteLine("Process " + p.getId() + " could not be removed");
                            }
                        }
                    }
                }

                return monitoredProcesses;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                if (sw != null)
                {
                    sw.Dispose();
                }
                if (aTimer != null)
                {
                    aTimer.Stop();
                    aTimer.Dispose();
                }

                return null;
            }
            finally
            {
                foundProcesses.Clear();
            }

        }

    }
}
