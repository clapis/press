using Press.Core;
using Press.Hosts.WebAPI.Endpoints;
using Press.Hosts.WebAPI.Jobs;
using Press.Hosts.WebAPI.OpenTelemetry;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.MongoDb.Configuration;
using Press.Infrastructure.Postmark;
using Press.Infrastructure.Postmark.Configuration;
using Press.Infrastructure.Scrapers;

var builder = WebApplication.CreateBuilder(args);

T GetSettings<T>(string key) => builder.Configuration.GetRequiredSection(key).Get<T>()!;

builder.Services
    .AddCore()
    .AddScrapers()
    .AddQuartzJobs()
    .AddMongoDb(GetSettings<MongoDbSettings>("MongoDb"))
    .AddPostmark(GetSettings<PostmarkSettings>("Postmark"));

builder.Services
    .AddOpenTelemetry(GetSettings<GrafanaCloudOptions>("Grafana"));

builder.Services
    .AddHealthChecks();

builder.Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication()
    .AddJwtBearer(opts =>
    {
        opts.Authority = "https://halyard.kinde.com";
        opts.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAlertEndpoints()
    .MapPublicationEndpoints()
    .MapHealthChecks("/healthz");

app.Run();


// Required by Component tests
public partial class Program { }

