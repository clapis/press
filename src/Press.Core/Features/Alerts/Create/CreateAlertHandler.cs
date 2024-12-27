using MediatR;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.Alerts.Create;

public class CreateAlertHandler(IAlertStore store) : IRequestHandler<CreateAlertRequest>
{
    private const int MaxNumberOfAlerts = 20;
    public async Task Handle(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var alerts = await store.GetByUserIdAsync(request.UserId, cancellationToken);
        
        // Ensure max number of alerts is not exceeded
        if (alerts.Count >= MaxNumberOfAlerts)
            throw new Exception("User {UserId} has exceeded max number of alerts.");
        
        // If there is already an alert for this keyword, ignore request
        if (alerts.Any(x => x.Term == request.Keyword)) 
            return;
        
        var alert = new Alert
        {
            Term = request.Keyword, 
            UserId = request.UserId
        };

        await store.InsertAsync(alert, cancellationToken);
    }
}