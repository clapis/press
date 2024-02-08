using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Press.Core.Publications;

namespace Press.Web.Pages.Components
{
    public class LastPublicationsViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;
        private readonly IPublicationStore _store;

        public LastPublicationsViewComponent(IMemoryCache cache, IPublicationStore store)
        {
            _cache = cache;
            _store = store;
        }
        
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var pubs = await GetLatestPublicationsBySourceAsync(cancellationToken);
            
            return View("/Pages/Components/LastPublications.cshtml", pubs);
        }
        
        private async Task<List<Publication>> GetLatestPublicationsBySourceAsync(CancellationToken cancellationToken)
        {
            return (await _cache.GetOrCreateAsync("max-date", async entry =>
            {
                var pubs = await _store.GetLatestPublicationsBySourceAsync(cancellationToken);

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

                return pubs;
            }))!;
        }

        
    }
}