using Quartz;

namespace Press.Hosts.WebAPI.Jobs;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(quartz =>
        {
            quartz.AddJobAndTrigger<ScrapeJob>("0 0 3/4 * * ?");
            quartz.AddJobAndTrigger<AlertJob>("0 10 3/4 * * ?");
            quartz.AddJobAndTrigger<ReportJob>("0 0 12 ? * SAT");
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
    
    private static void AddJobAndTrigger<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        string schedule)
        where T : IJob
    {
        var key = new JobKey(typeof(T).Name);
        quartz.AddJob<T>(opts => opts.WithIdentity(key));

        quartz.AddTrigger(opts => opts
            .ForJob(key)
            .WithCronSchedule(schedule));
        
        // quartz.AddTrigger(opts => opts.ForJob(key).StartNow());
    }
}