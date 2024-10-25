using Press.Core.Features.Notifications.Alert;
using Press.Core.Features.Notifications.Report;
using Press.Core.Features.Sources.Scrape;
using Press.Core.Features.System.MonitorDelay;
using Quartz;

namespace Press.Hosts.WebAPI.Jobs;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(quartz =>
        {
            quartz.AddJobAndTrigger<MediatrJob<AlertRequest>>("0 15 0,16,20 * * ?");
            quartz.AddJobAndTrigger<MediatrJob<ReportRequest>>("0 0 12 ? * SAT");
            quartz.AddJobAndTrigger<MediatrJob<MonitorDelayRequest>>("0 0 12 * * ?");
            quartz.AddJobAndTrigger<MediatrJob<ScrapeSourcesRequest>>("0 0 0,16,20 * * ?");
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);

        return services;
    }
    
    private static void AddJobAndTrigger<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        string schedule)
        where T : IJob
    {
        var key = new JobKey(typeof(T).FullName!);
        quartz.AddJob<T>(opts => opts.WithIdentity(key));

        quartz.AddTrigger(opts => opts
            .ForJob(key)
            .WithCronSchedule(schedule));
        
        // quartz.AddTrigger(opts => opts.ForJob(key).StartNow());
    }
}