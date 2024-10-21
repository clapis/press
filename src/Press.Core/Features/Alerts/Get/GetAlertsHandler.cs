using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Get;

public class GetAlertsHandler(IAlertStore store) : IRequestHandler<GetAlertsRequest, List<Alert>>
{
    public async Task<List<Alert>> Handle(GetAlertsRequest request, CancellationToken cancellationToken)
        => await store.GetByUserIdAsync(request.UserId, cancellationToken);
}