using MediatR;
using Press.Core.Domain;

namespace Press.Core.Features.Sources.Scrape;

public record SourcesScrapeRequest : IRequest
{
    public PublicationSource[] Sources { get; init; } = [ PublicationSource.Sorocaba, PublicationSource.SaoCarlos ];
}