using MediatR;

namespace Press.Core.Features.Alerts.Delete;

public record DeleteAlertRequest(string Id, string UserId) : IRequest;