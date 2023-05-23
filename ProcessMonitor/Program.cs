using System; 

namespace ProcessMonitor
{
    public class Program
    {

        public static void Main(string[] args)
        {
            string name;
            int threshold, frequency;
            IProcessService service;
            IHandler handler;
            ITimer aTimer;
            Monitor processMonitor;
            //Verify args
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid number of arguments");
                return;
            }
            try
            {
                name = args[0];
                threshold = int.Parse(args[1]);
                frequency = int.Parse(args[2]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid arguments: " + e.Message);
                return;
            }
            //Initialize monitor based on args
            service = new Service();
            handler = new Handler();
            aTimer = new SystemTimer(frequency * 60 * 1000);
            processMonitor = new Monitor(name, threshold, frequency, service, handler, aTimer);

            //start monitor
            processMonitor.Start();

            while (Console.KeyAvailable == false)
            {
                //Stop monitor if specific key is pressed
                if (Console.ReadKey().KeyChar == processMonitor.GetStopKey())
                {
                    processMonitor.Stop();
                    return;
                }
            }

        }
    }
}
