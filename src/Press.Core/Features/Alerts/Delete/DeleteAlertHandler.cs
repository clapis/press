using MediatR;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Delete;

public class DeleteAlertHandler(IAlertStore store) : IRequestHandler<DeleteAlertRequest>
{
    public async Task Handle(DeleteAlertRequest request, CancellationToken cancellationToken)
        => await store.DeleteAsync(request.Id, request.UserId, cancellationToken);
}