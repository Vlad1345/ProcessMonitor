using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProcessMonitor
{
    //Interface for methods that handle processes
    public interface IHandler
    {
        public bool KillProcess(int id, Dictionary<int, int> dict);

    }
    class Handler : IHandler
    {
        public bool KillProcess(int id, Dictionary<int, int> dict)
        {
            if (dict.ContainsKey(id))
            {
                Process.GetProcessById(id).Kill();
                return true;
            }
            return false;
        }
    }
}
