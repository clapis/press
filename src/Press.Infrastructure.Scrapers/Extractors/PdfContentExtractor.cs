using System.Text;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Extractors;

public class PdfContentExtractor(HttpClient client) : IPdfContentExtractor
{
    public async Task<string> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        using var directory = new DisposableDirectory();

        var filePath = Path.Combine(directory.Path, "file.pdf");

        await DownloadFileAsync(url, filePath, cancellationToken);

        return ExtractPdfText(filePath);
    }

    private async Task DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var file = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, true);
        
        await stream.CopyToAsync(file, cancellationToken);
    }

    private string ExtractPdfText(string filePath)
    {
        var result = new StringBuilder();
        
        using var docReader = Docnet.Core.DocLib.Instance.GetDocReader(filePath, new Docnet.Core.Models.PageDimensions());
        
        for (var i = 0; i < docReader.GetPageCount(); i++)
        {
            using (var pageReader = docReader.GetPageReader(i))
                result.Append(pageReader.GetText());
        }
        
        return result.ToString();
    }
}