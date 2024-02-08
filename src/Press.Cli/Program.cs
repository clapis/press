using Microsoft.Extensions.Logging.Abstractions;
using Press.Scrapers.SaoCarlos;

var provider = new PublicationProvider(NullLogger<PublicationProvider>.Instance);

var publications = provider.ProvideAsync(CancellationToken.None);

await foreach (var publication in publications)
{
    Console.WriteLine($"{publication.Date.ToShortDateString()}\t{publication.Url}");
};

Console.WriteLine("done");