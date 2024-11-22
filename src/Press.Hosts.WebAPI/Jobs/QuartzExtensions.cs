using Press.Core.Features.Alerts.Run;
using Press.Core.Features.Sources.Scrape;
using Press.Core.Features.System;
using Quartz;

namespace Press.Hosts.WebAPI.Jobs;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        services.AddQuartz(quartz =>
        {
            quartz.AddJobAndTrigger<MediatrJob<RunAlerts>>("0 15 0,16,20 * * ?");
            quartz.AddJobAndTrigger<MediatrJob<ReportStaleSources>>("0 0 12 * * ?");
            quartz.AddJobAndTrigger<MediatrJob<ReportSourcesStatus>>("0 0 12 ? * SAT");
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