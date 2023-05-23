using System; 
using System.Diagnostics;
using System.Collections.Generic;


namespace ProcessMonitor
{
    //Service that returns processes
    public interface IProcessService
    {
        public List<ProcessInfo> getProcesses(string processName);

    }
    public class Service : IProcessService
    {
        public List<ProcessInfo> getProcesses(string processName)
        {
            List<Process> found = null;
            List<ProcessInfo> toReturn = new List<ProcessInfo>();
            try
            {
                found = new List<Process>(Process.GetProcessesByName(processName));
                foreach (Process p in found)
                {
                    toReturn.Add(new ProcessInfo(p.Id));
                }
                return toReturn;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                if (found != null)
                {
                    found.Clear();
                }
                if (toReturn != null)
                {
                    toReturn.Clear();
                }
                return null;
            }
        }
    }
}
