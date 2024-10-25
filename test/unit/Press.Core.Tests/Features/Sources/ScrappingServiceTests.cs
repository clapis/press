using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Press.Core.Domain;
using Press.Core.Features.Sources.Scrape;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Tests.Features.Sources;

public class ScrappingServiceTests
{
    private readonly ScrappingService _target;

    private readonly Mock<IPublicationStore> _store = new();
    private readonly Mock<IPublicationProvider> _provider = new();
    private readonly Mock<IPdfContentExtractor> _extractor = new();

    public ScrappingServiceTests()
    {
        _target = new ScrappingService(_store.Object, _provider.Object, _extractor.Object, NullLogger<ScrappingService>.Instance);
    }

    [Fact(DisplayName = "When a document has already been scraped, skip it")]
    public async Task Test01()
    {
        var source = new Source { Id = "test-01" };
        
        // Document 1 has already been scrapped
        _store
            .Setup(x => x.GetLatestUrlsAsync(source, It.IsAny<CancellationToken>()))
            .ReturnsAsync([$"https://example.com/document-1.pdf"]);
        
        // Document 1,2,3 are returned from provider
        _provider
            .Setup(x => x.ProvideAsync(source, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable
                .Range(1, 3)
                .Select(i => new Publication { Url = $"https://example.com/document-{i}.pdf" })
                .ToList());
        
        _extractor
            .Setup(x => x.ExtractAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string url, CancellationToken _) => $"Extracted contents of url: {url}");
        
        await _target.ScrapeAsync(source, CancellationToken.None);

        // Verify we don't attempt to extract contents of document 1
        _extractor.Verify(x
                => x.ExtractAsync(It.Is<string>(url => url.EndsWith("document-1.pdf")), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}