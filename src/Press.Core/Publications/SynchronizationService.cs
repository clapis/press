using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Press.Core.Publications
{
    public class SynchronizationService
    {
        private readonly IPublicationStore _store;
        private readonly IContentExtractor _extractor;
        private readonly IEnumerable<IPublicationProvider> _providers;
        private readonly ILogger<SynchronizationService> _logger;

        public SynchronizationService(
            IPublicationStore store,
            IContentExtractor extractor,
            IEnumerable<IPublicationProvider> providers, 
            ILogger<SynchronizationService> logger)
        {
            _store = store;
            _extractor = extractor;
            _providers = providers;
            _logger = logger;
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
                publication.Contents = await _extractor.ExtractAsync(publication.Url, cancellationToken);

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
    }
}