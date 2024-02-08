using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Press.Core.Publications
{
    public class SynchronizationService
    {
        private readonly IPublicationStore _store;
        private readonly IContentExtractor _extractor;
        private readonly IPublicationScraper _provider;
        private readonly ILogger<SynchronizationService> _logger;

        public SynchronizationService(
            IPublicationStore store,
            IContentExtractor extractor,
            IPublicationScraper provider, 
            ILogger<SynchronizationService> logger)
        {
            _store = store;
            _extractor = extractor;
            _provider = provider;
            _logger = logger;
        }

        public async Task SynchronizeAsync(CancellationToken cancellationToken)
        {
            // Get all stored publication urls, so we just download new publications
            var urls = await _store.GetAllUrlsAsync(cancellationToken);
            var stored = new HashSet<string>(urls, StringComparer.OrdinalIgnoreCase);

            // Scrape sources for publications
            var scraped = _provider.ScrapeAsync(cancellationToken);

            await foreach (var scrape in scraped)
            {
                // Exclude publications we already have downloaded
                if (stored.Contains(scrape.Link))
                    continue;

                // Extract contents 
                var contents = await _extractor.ExtractAsync(scrape.Link, cancellationToken);

                var publication = new Publication
                {
                    Url = scrape.Link,
                    Contents = contents,
                    Date = scrape.Date
                };

                // Store the publication
                await _store.SaveAsync(publication, cancellationToken);
            }
        }
    }
}