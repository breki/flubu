using System;
using System.Collections.Generic;
using System.Text;

namespace Flubu.Tasks.Misc
{
    public class SleepTask : TaskBase
    {
        public override string Description
        {
            get 
            { 
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Sleep for {0} seconds.", 
                    sleepPeriod.TotalSeconds); 
            }
        }

        public SleepTask (TimeSpan sleepPeriod)
        {
            this.sleepPeriod = sleepPeriod;
        }

        public static void Execute (ITaskContext context, TimeSpan sleepPeriod)
        {
            SleepTask task = new SleepTask (sleepPeriod);
            task.Execute (context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            System.Threading.Thread.Sleep (sleepPeriod);
        }

        private TimeSpan sleepPeriod;
    }
}
