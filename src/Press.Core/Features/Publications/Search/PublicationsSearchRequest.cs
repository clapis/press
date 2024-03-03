using MediatR;
using Press.Core.Domain;

namespace Press.Core.Features.Publications.Search;

public record PublicationsSearchRequest(string Query) : IRequest<List<Publication>>;