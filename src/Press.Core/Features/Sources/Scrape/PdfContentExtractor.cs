using UglyToad.PdfPig;

namespace Press.Core.Features.Sources.Scrape;

public interface IPdfContentExtractor
{
    Task<string> ExtractAsync(string url, CancellationToken cancellationToken);
}

public class PigExtractor(HttpClient client) : IPdfContentExtractor
{
    public async Task<string> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        var data = await client.GetByteArrayAsync(url, cancellationToken);

        using var doc = PdfDocument.Open(data);

        var contents = doc.GetPages()
            .SelectMany(page => page.GetWords()
                .Select(x => x.Text));

        return string.Join(" ", contents);
    }
}