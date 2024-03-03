using MediatR;
using Press.Core.Domain;

namespace Press.Core.Features.Publications.GetLatestBySource;

public record GetLatestPublicationsBySourceRequest : IRequest<List<Publication>>;