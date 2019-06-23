using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace ProcessRepeatData
{
    public class Job
    {
        public System.Timers.Timer Timer { get; set; }

        public DateTime StartTime { get; set; }
    }

    public class Schedule
    {
        public List<Job> Items { get; set; }


        public Schedule()
        {
            Items = new List<Job>();
        }



        public Schedule Add(int Interval, DateTime startTime, ElapsedEventHandler process)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = Interval;
            timer.Elapsed += process;
            Items.Add(new Job() { Timer = timer, StartTime = startTime });
            return this;
        }

        public void Start()
        {
            foreach (var t in Items)
            {
                while (true)
                {
                    if (t.StartTime < DateTime.Now)
                    {
                        t.Timer.Start();
                        break;
                    }
                    System.Threading.Thread.Sleep(1000 * 60);
                }
            }
        }
    }
}
