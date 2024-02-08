using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace Press.Core.Features.Sources.Scrape.Extractors;

public class PigExtractor : IPdfContentExtractor
{
    private readonly HttpClient _client;
    private readonly ILogger<PigExtractor> _logger;

    public PigExtractor(HttpClient client, ILogger<PigExtractor> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        var data = await _client.GetByteArrayAsync(url, cancellationToken);

        using var doc = PdfDocument.Open(data);

        var contents = doc.GetPages()
            .SelectMany(page => page.GetWords()
                .Select(x => x.Text));

        return string.Join(" ", contents);
    }
}