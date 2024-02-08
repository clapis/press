using Microsoft.Extensions.Logging;
using PdfSharpTextExtractor;
using Press.Core.Publications;

namespace Press.Scrapers
{
    public class ContentExtractor : IContentExtractor
    {
        private readonly HttpClient _client;
        private readonly ILogger<ContentExtractor> _logger;

        public ContentExtractor(HttpClient client, ILogger<ContentExtractor> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<string> ExtractAsync(string link, CancellationToken cancellationToken)
        {
            var name = link.Substring(link.LastIndexOf("/", StringComparison.Ordinal) + 1);

            var path = Path.Combine(Path.GetTempPath(), name);

            if (!File.Exists(path))
            {
                _logger.LogInformation("Downloading {Name} from {Link}", name, link);

                var data = await _client.GetByteArrayAsync(link, cancellationToken);

                await File.WriteAllBytesAsync(path, data, cancellationToken);

                _logger.LogInformation("Downloaded {Name} from {Link}", name, link);
            }

            return await Extractor.PdfToTextAsync(path);
        }
    }
}