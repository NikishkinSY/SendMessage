using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using System.Threading;
using System.Collections.Specialized;

namespace SendMessage
{
    public static partial class Scheduler
    {
        static ISchedulerFactory scheluderFactory;
        internal static IScheduler scheduler { get; set; }
        internal static event EventHandler<SendMessageEventArgs> OnEvent;
        static void RiseEvent(object sender, SendMessageEventArgs e)
        { if (OnEvent != null) OnEvent(sender, e); }

        internal enum JobGroup
        {
            Modem
        }

        internal enum JobType
        {
            Ping,
            Reboot
        }

        static Scheduler()
        {
            scheluderFactory = new StdSchedulerFactory();
            scheduler = scheluderFactory.GetScheduler();
        }

        #region работа с планировщиком
        /// <summary>
        /// старт планировщика
        /// </summary>
        internal static void StartScheluder()
        {
            if (scheduler.InStandbyMode)
                scheduler.Start();
        }
        /// <summary>
        /// пауза планировщика
        /// </summary>
        internal static void PauseScheluder()
        {
            if (scheduler.IsStarted)
                scheduler.Standby();
        }
        /// <summary>
        /// остановка планировщика
        /// </summary>
        /// <param name="WaitForJobsToComplete">ожидать-ли завершения выполняемых задач</param>
        internal static void ShutdownScheluder(bool WaitForJobsToComplete)
        {
            if (scheduler.IsStarted || scheduler.InStandbyMode)
                scheduler.Shutdown(WaitForJobsToComplete);
        }
        #endregion

        #region работа с задачами
        internal static void ResumeAllJobs()
        {
            if (scheduler.IsStarted)
                scheduler.ResumeAll();
        }
        internal static void PauseAllJobs()
        {
            if (scheduler.IsStarted)
                scheduler.PauseAll();
        }
        internal static void StopAllJobs()
        {
            if (scheduler.IsStarted || scheduler.InStandbyMode)
                scheduler.Clear();
        }
        #endregion

        #region работа с триггерами
        static ITrigger CreateTrigger(DateTime DTNext)
        {
            // computer a time that is on the next round minute
            DateTimeOffset runTime = new DateTimeOffset(DTNext);

            // Trigger the job to run on the next round minute
            ITrigger trigger = TriggerBuilder.Create()
                .StartAt(runTime)
                .Build();

            return trigger;
        }
        #endregion

        #region работа с задачей
        static JobKey CreateJobKey(JobType jobType, JobGroup jobGroup, object obj)
        {
            return new JobKey(String.Format("{0}.{1}", jobType, obj), jobGroup.ToString());
        }

        public static bool IsExistsJob(JobKey JobKey)
        {
            if (scheduler.IsStarted && scheduler.CheckExists(JobKey))
                return true;
            return false;
        }
        public static bool StopJob(JobKey JobKey)
        {
            if (IsExistsJob(JobKey))
                return scheduler.DeleteJob(JobKey);
            else
                return false;
        }
        public static bool IsJobCurrentlyExecuting(JobKey JobKey)
        {
            if (IsExistsJob(JobKey))
                if (scheduler.GetCurrentlyExecutingJobs().Count(s =>
                    s.JobDetail.Key == JobKey) > 0)
                    return true;
            return false;
        }
        #endregion
    }
}