using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewCompyTimer
{
    public class PeggedResourceCounter : INotifyPropertyChanged
    {
        private DateTime initTime = DateTime.Now;
        public double WaitPercent => TotalWaitTime.TotalMilliseconds / (TotalTime.TotalMilliseconds + 1);
        public TimeSpan TotalTime => DateTime.Now - initTime;
        public TimeSpan TotalWaitTime { get; set; } = new TimeSpan();
        public decimal HourlyRate { get; set; } = 50;
        public decimal TotalWaitCost => (decimal)TotalWaitTime.TotalHours * HourlyRate;
        //private Stopwatch timer = new Stopwatch();
        private PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
        private PerformanceCounter disk = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        const int intervalMs = 50;

        public float CpuUsage => cpu.NextValue();
        public float DiskUsage => disk.NextValue();

        public event PropertyChangedEventHandler PropertyChanged;

        public PeggedResourceCounter()
        {
            Task.Run(ResourceCheckLoop);
        }


        private async Task ResourceCheckLoop()
        {
            var i = 1;
            while (true)
            {
                if (CheckIfPegged())
                {
                    TotalWaitTime = TotalWaitTime.Add(TimeSpan.FromMilliseconds(intervalMs));
                }
                if (++i % 200 == 0)
                {
                    i = 0;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
                await Task.Delay(intervalMs);
            }
        }

        private bool CheckIfPegged()
        {
            return cpu.NextValue() > 33 || disk.NextValue() > 50;
        }
    }
}
