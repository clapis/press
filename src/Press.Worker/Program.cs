using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Press.Core;
using Press.Core.Alerts;
using Press.MongoDb;
using Press.MongoDb.Configuration;
using Press.Scrapers;
using Press.Worker.Configuration;
using Press.Worker.Extensions;
using Press.Worker.Jobs;
using Press.Worker.Services;
using Quartz;

namespace Press.Worker
{
    public class Program
    {
        public static void Main(string [] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string [] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) => 
                    builder.AddSeq(context.Configuration.GetSection("Seq")))
                .ConfigureServices((context, services) =>
                {
                    var mongo = context.Configuration.GetSection("MongoDb").Get<MongoDbOptions>();
                    var postmark = context.Configuration.GetSection("PostMark").Get<PostMarkSettings>();
                    
                    services.AddScrapers();
                    services.AddPressCore();
                    services.AddMongoPress(mongo);

                    services.AddSingleton(postmark);
                    services.AddSingleton<INotificationService, EmailNotificationService>();

                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionScopedJobFactory();
                        
                        q.AddJobAndTrigger<SyncJob>(context.Configuration);
                        q.AddJobAndTrigger<AlertJob>(context.Configuration);
                        q.AddJobAndTrigger<ReportJob>(context.Configuration);
                    });

                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

                });
    }
}