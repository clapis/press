﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Press.Core;
using Press.Core.Features.Publications.GetLatestBySource;
using Press.Core.Features.Publications.Search;
using Press.Core.Features.Sources.Scrape;
using Press.Hosts.Cli.Extensions;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.MongoDb.Configuration;
using Press.Infrastructure.Postmark;
using Press.Infrastructure.Postmark.Configuration;
using Press.Infrastructure.Scrapers;

await BuildCommandLine()
    .UseHost(_ => Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            T GetSettings<T>(string key) => context.Configuration.GetRequiredSection(key).Get<T>()!;

            services
                .AddCore()
                .AddScrapers()
                .AddMongoDb(GetSettings<MongoDbSettings>("MongoDb"))
                .AddPostmark(GetSettings<PostmarkSettings>("Postmark"));
        }))
    .UseDefaults()
    .Build()
    .InvokeAsync(args);

// press-cli sources list
// press-cli sources enable [source]
// press-cli sources disable [source]
// press-cli sources scrape --all
// press-cli sources scrape [source]

// press-cli publications latest
// press-cli publications search --query [term]

static CommandLineBuilder BuildCommandLine()
{
    var root = new RootCommand("Press CLI");

    root
        .AddSubcommand("sources", opts => opts
            .AddSubcommand<ScrapeSourcesRequest>("scrape", "Scrapes a given source for new publications"))
        .AddSubcommand("publications", opts => opts
            .AddSubcommand<PublicationsSearchRequest>("search", "Search scraped publications for a given term")
            .AddSubcommand<GetLatestPublicationsBySourceRequest>("latest", "Returns latest publications by source"));

    return new CommandLineBuilder(root);
}
