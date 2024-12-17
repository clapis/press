using Press.Core;
using Press.Hosts.WebAPI.Endpoints;
using Press.Hosts.WebAPI.Jobs;
using Press.Hosts.WebAPI.Telemetry;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.Postmark;
using Press.Infrastructure.Scrapers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCore()
    .AddQuartzJobs()
    .AddMongoDb(GetSettings<MongoDbSettings>("MongoDb"))
    .AddPostmark(GetSettings<PostmarkSettings>("Postmark"))
    .AddTelemetry(GetSettings<TelemetryOptions>("Telemetry"))
    .AddScrapers(GetSettings<ScrapersSettings>("Scrapers"));

builder.Services
    .AddAuthorization()
    .AddAuthentication()
    .AddJwtBearer(builder.Configuration.GetSection("Authorization:Jwt").Bind);

builder.Services
    .AddHealthChecks();

builder.Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();

builder.Services
    .AddOutputCache();

T GetSettings<T>(string key) => builder.Configuration.GetRequiredSection(key).Get<T>()!;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOutputCache();

app.MapHealthChecks("/healthz");

app.MapAlertEndpoints()
    .MapSourceEndpoints()
    .MapSystemEndpoints()
    .MapWebhookEndpoints()
    .MapPublicationEndpoints();

app.Run();


// Required by Component tests
public partial class Program { }

