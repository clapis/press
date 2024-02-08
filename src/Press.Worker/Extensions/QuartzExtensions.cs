using Quartz;

namespace Press.Worker.Extensions
{
    // https://andrewlock.net/using-quartz-net-with-asp-net-core-and-worker-services/
    public static class QuartzExtensions
    {
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            IConfiguration config)
            where T : IJob
        {
            // Use the name of the IJob as the appsettings.json key
            string jobName = typeof(T).Name;

            // Try and load the schedule from configuration
            var configKey = $"Quartz:{jobName}";
            var cronSchedule = config[configKey];

            // Some minor validation
            if (string.IsNullOrEmpty(cronSchedule))
                throw new Exception($"No Quartz.NET Cron schedule found for job in configuration at {configKey}");

            // register the job as before
            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithCronSchedule(cronSchedule));

            // quartz.AddTrigger(opts => opts.ForJob(jobKey).StartNow());
        }
    }
}