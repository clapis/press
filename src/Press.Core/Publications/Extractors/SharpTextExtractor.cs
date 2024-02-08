using Microsoft.Extensions.Logging;
using PdfSharpTextExtractor;

namespace Press.Core.Publications.Extractors
{
    public class SharpTextExtractor : IContentExtractor
    {
        private readonly HttpClient _client;
        private readonly ILogger<SharpTextExtractor> _logger;

        public SharpTextExtractor(HttpClient client, ILogger<SharpTextExtractor> logger)
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