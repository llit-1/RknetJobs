using Quartz;
using Quartz.Impl;

namespace RknetJobs.Jobs.SheduledTasks
{
    public class ScheduledTaskService: IHostedService
    {

        private IScheduler _scheduler;
            public Task StartAsync(CancellationToken cancellationToken)
        {
            StdSchedulerFactory factory = new();
            _scheduler = factory.GetScheduler().Result;
            IJobDetail job = JobBuilder.Create<GetGateAccessDataJob>().WithIdentity("myJob", "myGroup").Build();
            ITrigger trigger = TriggerBuilder.Create().WithIdentity("myTrigger", "myGroup")
                                                 .WithSimpleSchedule(x => x.WithIntervalInHours(3).RepeatForever())
                                                 .Build();
            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start().Wait();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler.Shutdown(waitForJobsToComplete: true);
            return Task.CompletedTask;
        }
    }
}
