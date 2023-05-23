using System; 
using System.Diagnostics;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using ProcessMonitor;
using System.Timers;

namespace NUnitTestProcessMonitor
{
    [TestFixture]
    public class Tests
    {

        private Mock<IProcessService> service;
        private Mock<IHandler> handler;
        private Mock<ITimer> aTimer;
        private Monitor monitor;
        //custom values for testing
        private const int threshold = 5, frequency = 2;

        //set up monitor
        [OneTimeSetUp]
        public void Setup()
        {
            service = new Mock<IProcessService>(MockBehavior.Strict);
            handler = new Mock<IHandler>(MockBehavior.Strict);
            aTimer = new Mock<ITimer>(MockBehavior.Strict);
            monitor = new Monitor("notepad", threshold, frequency, service.Object, handler.Object, aTimer.Object);
        }


        //test if new processes are added to monitor 
        [Test]
        public void Test1()
        {
            //Arrange
            Dictionary<int, int> monitored = new Dictionary<int, int>();
            int Id1 = 1, Id2 = 2, Id3 = 3;
            //service returns specified list of processes
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id2), new ProcessInfo(Id3) });
            //mock terminating processes
            handler.Setup(h => h.KillProcess(It.IsAny<int>(), It.IsAny<Dictionary<int, int>>())).Returns(true);

            //Act
            monitored = monitor.UpdateProcesses(monitored, service.Object.getProcesses(It.IsAny<string>()));

            //Assert
            Assert.AreEqual(3, monitored.Count);
            Assert.AreEqual(true, monitored.ContainsKey(Id1));
            Assert.AreEqual(true, monitored[Id1] == 0);
            Assert.AreEqual(true, monitored.ContainsKey(Id2));
            Assert.AreEqual(true, monitored[Id2] == 0);
            Assert.AreEqual(true, monitored.ContainsKey(Id3));
            Assert.AreEqual(true, monitored[Id3] == 0);
        }


        //test if processes are removed once they exceed threshold
        [Test]
        public void Test2()
        {
            //Arrange
            Dictionary<int, int> monitored = new Dictionary<int, int>();
            int Id1 = 1, Id2 = 2, Id3 = 3;
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id2), new ProcessInfo(Id3) });
            handler.Setup(h => h.KillProcess(It.IsAny<int>(), It.IsAny<Dictionary<int, int>>())).Returns(true);
            //Act

            //number of times a monitor needs to find a process for it to exceed limit 
            //(including the first time when the monitor finds the task)
            int steps = (int)Math.Ceiling((float)threshold / (float)frequency);
            steps++;

            for (int i = 0; i < steps; i++)
            {
                monitored = monitor.UpdateProcesses(monitored, service.Object.getProcesses(It.IsAny<string>()));
                //service return values need to be assigned after each UpdateProcesses call
                service.Setup(s => s.getProcesses(It.IsAny<string>()))
                            .Returns(new List<ProcessInfo>() { new ProcessInfo(1), new ProcessInfo(2), new ProcessInfo(3) });
            }

            //Assert
            Assert.AreEqual(0, monitored.Count);
        }


        //test if monitor removes processes that no longer exist
        [Test]
        public void Test3()
        {
            //Arrange
            Dictionary<int, int> monitored = new Dictionary<int, int>();
            int Id1 = 1, Id2 = 2, Id3 = 3;
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id2), new ProcessInfo(Id3) });
            handler.Setup(h => h.KillProcess(It.IsAny<int>(), It.IsAny<Dictionary<int, int>>())).Returns(true);

            //Act
            monitored = monitor.UpdateProcesses(monitored, service.Object.getProcesses(It.IsAny<string>()));
            //processes with ids Id1 and Id3 not returned by service
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id2) });
            monitored = monitor.UpdateProcesses(monitored, service.Object.getProcesses(It.IsAny<string>()));

            //Assert
            Assert.AreEqual(1, monitored.Count);
            Assert.AreEqual(false, monitored.ContainsKey(Id1));
            Assert.AreEqual(true, monitored.ContainsKey(Id2));
            Assert.AreEqual(false, monitored.ContainsKey(Id3));
            Assert.AreEqual(true, monitored[Id2] == frequency);

        }


        //Test that monitor updates dictionary on timer interval 
        [Test]
        public void Test4()
        {
            //Arrange 
            Dictionary<int, int> monitored;
            int Id1 = 1, Id2 = 2, Id3 = 3;

            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id2), new ProcessInfo(Id3) });
            handler.Setup(h => h.KillProcess(It.IsAny<int>(), It.IsAny<Dictionary<int, int>>())).Returns(true);
            //mock starting timer
            aTimer.Setup(t => t.StartWithReset()).Returns(true);

            //Act

            //start monitor
            monitor.Start();

            //processes checked on monitor start so return values reassigned
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id2), new ProcessInfo(Id3) });
            //mock elapsed timer interval
            aTimer.Raise(item => item.Elapsed += null, new EventArgs() as ElapsedEventArgs);
            //remove Id2 process
            service.Setup(s => s.getProcesses(It.IsAny<string>()))
                                .Returns(new List<ProcessInfo>() { new ProcessInfo(Id1), new ProcessInfo(Id3) });
            //mock elapsed timer interval
            aTimer.Raise(item => item.Elapsed += null, new EventArgs() as ElapsedEventArgs);

            //Assert

            //get monitor dictionary
            monitored = monitor.GetMonitoredProcesses();

            Assert.AreEqual(2, monitored.Count);
            Assert.AreEqual(true, monitored[Id1] == frequency * 2);
            Assert.AreEqual(true, !monitored.ContainsKey(Id2));
            Assert.AreEqual(true, monitored[Id3] == frequency * 2);
        }
    }
}