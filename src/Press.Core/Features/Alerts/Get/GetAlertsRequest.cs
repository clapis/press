using MediatR;
using Press.Core.Domain;

namespace Press.Core.Features.Alerts.Get;

public record GetAlertsRequest(string UserId): IRequest<List<Alert>>;