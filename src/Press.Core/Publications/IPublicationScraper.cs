using System;
using System.Collections.Generic;
using System.Threading;

namespace Press.Core.Publications
{
    public interface IPublicationScraper
    {
        IAsyncEnumerable<ScrapedItem> ScrapeAsync(CancellationToken cancellationToken);
    }
    
    public record ScrapedItem(DateTime Date, string Link);
}