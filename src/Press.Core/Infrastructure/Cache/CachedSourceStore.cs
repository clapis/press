using Microsoft.Extensions.Caching.Memory;
using Press.Core.Domain;
using Press.Core.Infrastructure.Data;

namespace Press.Core.Infrastructure.Cache;

public interface ICachedSourceStore
{
    Task<Dictionary<string,Source>> GetSourceMapAsync(CancellationToken cancellationToken);
}

public class CachedSourceStore(ISourceStore store, IMemoryCache cache) : ICachedSourceStore
{
    public Task<Dictionary<string,Source>> GetSourceMapAsync(CancellationToken cancellationToken) 
        => cache.GetOrCreateAsync<Dictionary<string,Source>>("sources", async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromDays(1));
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            var sources = await store.GetAllAsync(cancellationToken);

            return sources.ToDictionary(x => x.Id);
        })!;
}