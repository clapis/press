using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Press.Core.Domain;
using Press.Core.Features.Sources.Scrape;
using Press.Core.Features.Sources.Scrape.Extractors;
using Press.Core.Infrastructure.Data;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Core.Tests.Features.Sources;

public class SourcesScrapeTests
{
    private readonly SourcesScrapeHandler _handler;

    private readonly Mock<IPublicationStore> _store = new();
    private readonly Mock<IPdfContentExtractor> _extractor = new();
    private readonly Dictionary<PublicationSource, Mock<IPublicationProvider>> _providers;

    public SourcesScrapeTests()
    {
        _providers = Enum.GetValues<PublicationSource>()
            .ToDictionary(source => source, source =>
            {
                var provider = new Mock<IPublicationProvider>();

                provider
                    .SetupGet(x => x.IsEnabled)
                    .Returns(true);
                
                provider
                    .SetupGet(x => x.Source)
                    .Returns(source);
                
                provider
                    .Setup(x => x.ProvideAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Enumerable
                        .Range(1, 3)
                        .Select(i => new Publication { Url = $"https://example.com/{source}/document-{i}.pdf" })
                        .ToList());

                return provider;
            });
        
        _handler = new SourcesScrapeHandler(_store.Object, _extractor.Object,
            _providers.Values.Select(x => x.Object), 
            NullLogger<SourcesScrapeHandler>.Instance);
    }
    

    [Fact(DisplayName = "When a document has already been scraped, skip it")]
    public async Task Test01()
    {
        _store
            .Setup(x => x.GetLatestUrlsAsync(It.IsAny<PublicationSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicationSource source, CancellationToken _) => [ $"https://example.com/{source}/document-1.pdf" ]);

        _extractor
            .Setup(x => x.ExtractAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string url, CancellationToken _) => $"Extracted contents of url: {url}");

        var request = new SourcesScrapeRequest();
        
        await _handler.Handle(request, CancellationToken.None);
        
        _extractor.Verify(x 
                => x.ExtractAsync(It.Is<string>(url => url.EndsWith("document-1.pdf")), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact(DisplayName = "When content extraction fails, skip it")]
    public async Task Test02()
    {
        _store
            .Setup(x => x.GetLatestUrlsAsync(It.IsAny<PublicationSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _extractor
            .Setup(x => x.ExtractAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to extract document contents"));

        await _handler.Handle(new SourcesScrapeRequest(), CancellationToken.None);
        
        _store.Verify(x 
            => x.SaveAsync(It.IsAny<Publication>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}