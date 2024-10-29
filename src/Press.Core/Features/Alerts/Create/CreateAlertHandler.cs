using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Create;

public class CreateAlertHandler(IAlertStore store) : IRequestHandler<CreateAlertRequest>
{
    public async Task Handle(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = new Alert
        {
            Term = request.Keyword, 
            UserId = request.UserId
        };

        await store.InsertAsync(alert, cancellationToken);
    }
}