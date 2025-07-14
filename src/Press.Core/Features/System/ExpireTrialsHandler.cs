using MediatR;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Features.System;

public record ExpireTrials : IRequest;

public class ExpireTrialsHandler(IUserStore store) : IRequestHandler<ExpireTrials>
{
    public async Task Handle(ExpireTrials request, CancellationToken cancellationToken)
        => await store.ExpireTrialsAsync(cancellationToken);
}