using System; 
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor
{
    //Holds information returned by the service
    public class ProcessInfo
    {
        private int Id;
        public int getId() { return Id; }
        public ProcessInfo(int Id)
        {
            this.Id = Id;
        }
    }
}
