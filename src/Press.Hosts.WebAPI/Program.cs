using MediatR;
using Microsoft.AspNetCore.Mvc;
using Press.Core;
using Press.Core.Features.Publications.GetLatestBySource;
using Press.Core.Features.Publications.Search;
using Press.Core.Features.Sources.Scrape;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");

app.MapPost("/sources/scrape",
    async ([FromServices] IMediator mediator, CancellationToken cancellationToken)
        => await mediator.Send(new SourcesScrapeRequest(), cancellationToken));

app.MapGet("/publications/search",
    async ([FromServices] IMediator mediator, [FromQuery(Name = "q")] string query, CancellationToken cancellationToken)
        => await mediator.Send(new PublicationsSearchRequest(query), cancellationToken));

app.MapGet("/publications/latest-by-source",
    async ([FromServices] IMediator mediator, CancellationToken cancellationToken)
        => await mediator.Send(new GetLatestPublicationsBySourceRequest(), cancellationToken));

app.Run();