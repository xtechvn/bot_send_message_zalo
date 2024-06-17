using JOB_CRAWL_MESSAGE_ZALO;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Configuration;

namespace JOB_CRAWL_ZALO_MESSAGE
{
    class Program
    {
        private static int schedule_time_hours = Convert.ToInt32(ConfigurationManager.AppSettings["schedule_time_hours"]);
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //#region LISTENER 
            // construct a scheduler factory
            var schedFact = new StdSchedulerFactory();

            // get a scheduler, start the schedular before triggers or anything else
            IScheduler sched = schedFact.GetScheduler();
            sched.Start();

            // create job crawl today deal's
            IJobDetail job = JobBuilder.Create<ZaloJobCrawler>()
                        .WithIdentity("jobCrawlTodayDealAmazon", "groupAmazon")
                        .Build();

            // create trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "groupAmazon")
                .WithSimpleSchedule(x => x.WithIntervalInHours(9999999).RepeatForever())
                .Build();

            // Schedule the job using the job and trigger 
            sched.ScheduleJob(job, trigger);

            var myJobListener = new MyJobListener();
            myJobListener.Name = "CrawlListener";

            sched.ListenerManager.AddJobListener(myJobListener, KeyMatcher<JobKey>.KeyEquals(new JobKey("jobCrawlTodavb fgtgtrttrr5ytytbyhtbhj,yDealAmazon", "groupAmazon")));
            //#endregion  
        }
    }
}
