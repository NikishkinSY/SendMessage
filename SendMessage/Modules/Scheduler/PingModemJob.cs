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
        static IJobDetail CreatePingJob(Modem modem, JobKey jobKey)
        {
            IDictionary<string, object> Data = new Dictionary<string, object>();
            Data.Add("Modem", modem);

            JobDataMap JDM = new JobDataMap(Data);

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<PingJob>()
                .WithIdentity(jobKey)
                .UsingJobData(JDM)
                .Build();

            return job;
        }

        internal static void AddPingJob(Modem modem, DateTime DTNext)
        {
            JobKey jobKey = CreateJobKey(JobType.Ping, JobGroup.Modem, modem);
            StopJob(jobKey);
            IJobDetail job = CreatePingJob(modem, jobKey);

            // Trigger the job to run on the next round minute
            ITrigger trigger = CreateTrigger(DTNext);

            // Tell quartz to schedule the job using our trigger
            if (scheduler.InStandbyMode || scheduler.IsStarted)
                scheduler.ScheduleJob(job, trigger);
        }

        //ребут модемов
        class PingJob : IJob
        {
            public virtual void Execute(IJobExecutionContext context)
            {
                Modem modem = context.MergedJobDataMap.Get("Modem") as Modem;
                //пинг модема
                modem.PingModem();
                
                RiseEvent(this, new SendMessageEventArgs(EventType.Sys, MessageType.Note, "modem pinged"));

                //добавляем задачу для периодического выполнения
                Scheduler.AddPingJob(modem, DateTime.Now.Add(modem.PeriodPing));
            }
        }
    }
}
