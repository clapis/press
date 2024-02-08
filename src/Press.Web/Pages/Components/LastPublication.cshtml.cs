using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Press.Core.Publications;

namespace Press.Web.Pages.Components
{
    public class LastPublicationViewComponent : ViewComponent
    {
        private readonly IMemoryCache _cache;
        private readonly IPublicationStore _store;

        public LastPublicationViewComponent(IMemoryCache cache, IPublicationStore store)
        {
            _cache = cache;
            _store = store;
        }
        
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var date = await GetLastPublicationDateAsync(cancellationToken);
            
            return View("/Pages/Components/LastPublication.cshtml", date);
        }
        
        private async Task<DateTime> GetLastPublicationDateAsync(CancellationToken cancellationToken)
        {
            return await _cache.GetOrCreateAsync("max-date", async entry =>
            {
                var max = await _store.GetMaxDateAsync(cancellationToken);

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

                return max;
            });
        }

        
    }
}