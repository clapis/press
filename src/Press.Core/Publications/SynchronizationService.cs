using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Press.Core.Publications
{
    public class SynchronizationService
    {
        private readonly IPublicationStore _store;
        private readonly IEnumerable<IContentExtractor> _extractors;
        private readonly IEnumerable<IPublicationProvider> _providers;
        private readonly ILogger<SynchronizationService> _logger;

        public SynchronizationService(
            IPublicationStore store,
            IEnumerable<IContentExtractor> extractors,
            IEnumerable<IPublicationProvider> providers, 
            ILogger<SynchronizationService> logger)
        {
            _store = store;
            _logger = logger;
            _providers = providers;
            _extractors = extractors;
        }

        public async Task SynchronizeAsync(CancellationToken cancellationToken)
        {
            // Get all stored publication urls, so we just download new publications
            var urls = await _store.GetAllUrlsAsync(cancellationToken);
            var stored = new HashSet<string>(urls, StringComparer.OrdinalIgnoreCase);

            // Scrape sources for publications
            await foreach (var publication in GetPublicationsAsync(cancellationToken))
            {
                // Exclude publications we already have downloaded
                if (stored.Contains(publication.Url))
                    continue;

                // Extract contents 
                publication.Contents = await ExtractContentsAsync(publication, cancellationToken);

                // Store the publication
                await _store.SaveAsync(publication, cancellationToken);
            }
        }

        private async IAsyncEnumerable<Publication> GetPublicationsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var provider in _providers)
                await foreach (var publication in provider.ProvideAsync(cancellationToken))
                    yield return publication;
        }

        private async Task<string> ExtractContentsAsync(Publication publication, CancellationToken token)
        {
            var extractions = await Task.WhenAll(_extractors
                .Select(extractor => extractor.ExtractAsync(publication.Url, token)));

            return string.Join(" ", extractions);
        }
    }
}