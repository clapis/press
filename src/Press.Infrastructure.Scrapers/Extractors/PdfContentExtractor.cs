using System.Text;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Extractors;

public class PdfContentExtractor : IPdfContentExtractor
{
    public Task<string> ExtractTextAsync(string filepath, CancellationToken cancellationToken)
    {
        var result = new StringBuilder();
        
        using var docReader = Docnet.Core.DocLib.Instance.GetDocReader(filepath, new Docnet.Core.Models.PageDimensions());
        
        for (var i = 0; i < docReader.GetPageCount(); i++)
        {
            using (var pageReader = docReader.GetPageReader(i))
                result.Append(pageReader.GetText());
        }
        
        return Task.FromResult(result.ToString());
    }
}