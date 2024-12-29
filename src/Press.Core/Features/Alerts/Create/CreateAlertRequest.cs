using MediatR;

namespace Press.Core.Features.Alerts.Create;

public record CreateAlertRequest(string Keyword, string SourceId, string UserId) : IRequest;