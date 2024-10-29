using Press.Core;
using Press.Hosts.WebAPI.Endpoints;
using Press.Hosts.WebAPI.Jobs;
using Press.Hosts.WebAPI.OpenTelemetry;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.Postmark;
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
    .AddAuthorization()
    .AddAuthentication()
    .AddJwtBearer(builder.Configuration.GetSection("Authorization:Jwt").Bind);

builder.Services
    .AddOpenTelemetry(GetSettings<GrafanaCloudOptions>("Grafana"));

builder.Services
    .AddHealthChecks();

builder.Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();

builder.Services
    .AddOutputCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOutputCache();

app.MapHealthChecks("/healthz");

app.MapAlertEndpoints()
    .MapPublicationEndpoints()
    .MapWebhookEndpoints();

app.Run();


// Required by Component tests
public partial class Program { }

