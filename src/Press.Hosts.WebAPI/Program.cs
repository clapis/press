using Microsoft.AspNetCore.Mvc;
using Press.Core;
using Press.Core.Features.Publications.GetLatestBySource;
using Press.Core.Features.Publications.Search;
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

builder.Logging
    .AddSeq(builder.Configuration.GetSection("Seq"));

builder.Services
    .AddOpenTelemetry()
    .ConfigureInstrumentation()
    .UseGrafanaCloudExporter(builder.Configuration.GetSection("Grafana"));

builder.Services
    .AddHealthChecks();

builder.Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");

app.MapGet("/publications/search",
    async ([FromServices] PublicationsSearchHandler handler, [FromQuery] string keyword, CancellationToken cancellationToken)
        => await handler.HandleAsync(keyword, cancellationToken));

app.MapGet("/publications/latest-by-source",
    async ([FromServices] GetLatestPublicationsBySourceHandler handler, CancellationToken cancellationToken)
        => await handler.HandleAsync(cancellationToken));

app.Run();