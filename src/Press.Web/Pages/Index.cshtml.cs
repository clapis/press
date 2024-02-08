using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Press.Core.Publications;

namespace Press.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IPublicationStore _store;

        public IndexModel(IPublicationStore store)
        {
            _store = store;
        }

        [BindProperty]
        public String SearchTerm { get; set; }

        public List<Publication> SearchResults { get; set; }

        public IActionResult OnGet(CancellationToken cancellationToken)
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(SearchTerm)) 
                return RedirectToPage();
            
            SearchResults = await _store.SearchAsync(SearchTerm, cancellationToken);

            return Page();
        }

    }
}