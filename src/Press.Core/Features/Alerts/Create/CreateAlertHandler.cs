using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Create;

public class CreateAlertHandler(IUserStore userStore, IAlertStore alertStore) : IRequestHandler<CreateAlertRequest>
{
    public async Task Handle(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var user = await userStore.GetByIdAsync(request.UserId, cancellationToken);
        var alerts = await alertStore.GetByUserIdAsync(request.UserId, cancellationToken);
        
        // Ensure max number of alerts is not exceeded
        if (user.Subscription is null || alerts.Count >= user.Subscription.MaxAlerts)
            throw new Exception("User {UserId} has exceeded max number of alerts.");
        
        var alert = new Alert
        {
            UserId = request.UserId,
            Keyword = request.Keyword, 
            SourceId = request.SourceId
        };

        await alertStore.InsertAsync(alert, cancellationToken);
    }
}