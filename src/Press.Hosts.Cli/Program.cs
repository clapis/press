using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Press.Core;
using Press.Core.Features.Publications.Search;
using Press.Core.Features.Sources.Get;
using Press.Core.Features.Sources.Scrape;
using Press.Hosts.Cli.Extensions;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.Postmark;
using Press.Infrastructure.Scrapers;

await BuildCommandLine()
    .UseHost(_ => Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            T GetSettings<T>(string key) => context.Configuration.GetRequiredSection(key).Get<T>()!;

            services
                .AddCore()
                .AddMongoDb(GetSettings<MongoDbSettings>("MongoDb"))
                .AddPostmark(GetSettings<PostmarkSettings>("Postmark"))
                .AddScrapers(GetSettings<ScrapersSettings>("Scrapers"));
        }))
    .UseDefaults()
    .Build()
    .InvokeAsync(args);

// press-cli sources list
// press-cli sources enable [source]
// press-cli sources disable [source]
// press-cli sources scrape --all
// press-cli sources scrape [source]
// press-cli sources status

// press-cli publications search --query [keyword]

static CommandLineBuilder BuildCommandLine()
{
    var root = new RootCommand("Press CLI");

    root
        .AddSubcommand("sources", opts => opts
            .AddSubcommand<ScrapeSourcesRequest>("scrape", "Scrapes sources for new publications"))
            .AddSubcommand<GetSourcesRequest>("status", "Returns latest publications by source")
        .AddSubcommand("publications", opts => opts
            .AddSubcommand<SearchPublicationsRequest>("search", "Search scraped publications for a given keyword"));

    return new CommandLineBuilder(root);
}
