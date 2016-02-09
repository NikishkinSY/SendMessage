using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;

namespace SendMessage
{
    public static partial class Scheduler
    {
        static IJobDetail CreateRebootJob(Modem modem, JobKey jobKey)
        {
            IDictionary<string, object> Data = new Dictionary<string, object>();
            Data.Add("Modem", modem);

            JobDataMap JDM = new JobDataMap(Data);

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<RebootJob>()
                .WithIdentity(jobKey)
                .UsingJobData(JDM)
                .Build();

            return job;
        }

        internal static void AddRebootJob(Modem modem, DateTime DTNext)
        {
            JobKey jobKey = CreateJobKey(JobType.Ping, JobGroup.Modem, modem);
            StopJob(jobKey);
            IJobDetail job = CreateRebootJob(modem, jobKey);

            // Trigger the job to run on the next round minute
            ITrigger trigger = CreateTrigger(DTNext);

            // Tell quartz to schedule the job using our trigger
            if (scheduler.InStandbyMode || scheduler.IsStarted)
                scheduler.ScheduleJob(job, trigger);
        }

        //ребут модемов
        class RebootJob : IJob
        {
            public virtual void Execute(IJobExecutionContext context)
            {
                Modem modem = context.MergedJobDataMap.Get("Modem") as Modem;
                //перегрузка модема
                modem.RebootModem();
                
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Note, "modem reseted"));

                //добавляем задачу для периодического выполнения
                Scheduler.AddRebootJob(modem, DateTime.Now.Add(modem.PeriodReboot));
            }
        }
    }
}
