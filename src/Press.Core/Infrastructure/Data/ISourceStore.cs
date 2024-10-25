using Press.Core.Domain;

namespace Press.Core.Infrastructure.Data;

public interface ISourceStore
{
     Task<List<Source>> GetAllAsync(CancellationToken cancellationToken);
}