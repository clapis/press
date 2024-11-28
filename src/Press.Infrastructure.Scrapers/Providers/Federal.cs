using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Press.Core.Domain;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Infrastructure.Scrapers.Providers;

// https://pesquisa.in.gov.br/imprensa/core/start.action
public class Federal(
    HttpClient httpClient,
    IPdfContentExtractor extractor) : IPublicationScraper
{
    public string SourceId => "dou";
    
    public async IAsyncEnumerable<Publication> ScrapeAsync(List<string> existing,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var dates = Enumerable.Range(0, 2)
            .Select(x => DateTime.UtcNow.Date.AddDays(-x))
            .Reverse();

        foreach (var date in dates)
        {
            var links = await ProvideLinksAsync(date, cancellationToken);

            foreach (var link in links.ExceptBy(existing, RemoveQueryString))
            {
                var contents = await extractor.ExtractAsync(link, cancellationToken);

                yield return new Publication
                {
                    SourceId = SourceId,
                    Url = RemoveQueryString(link),
                    Contents = contents,
                    Date = date,
                };
            }
        }
    }

    private async Task<IEnumerable<string>> ProvideLinksAsync(DateTime date, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsync("https://pesquisa.in.gov.br/imprensa/core/jornalList.action", 
            new FormUrlEncodedContent(SearchForm(date)), cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var matches = Regex.Matches(content, "https:\\/\\/download\\.in\\.gov\\.br\\/.*\\.pdf[^']*");

        return matches.Select(x => System.Net.WebUtility.HtmlDecode(x.Value));
    }

    private static IEnumerable<KeyValuePair<string, string>> SearchForm(DateTime date) =>
    [
        new ("edicao.jornal_hidden","1,1000,1010,1020,515,521,522,531,535,536,523,532,540,1040,2,2000,529,525,3,3000,3020,1040,526,530,600,601,602,603,604,605,606,607,608,609,610,611,612,613,614,615,616,617,618,619,620,621,622,623,624,625,626,627,628,629,630,631,632,633,634,635,636,637,638,639,640,641,642,643,644,645,646,647,648,649,650,651,652,653,654,655,656,657,658,659,660,661,662,663,664,665,666,667,668,669,670,671,672,673,674,675,676,677,701,702"),
        new ("__checkbox_edicao.jornal","1,1000,1010,1020,515,521,522,531,535,536,523,532,540,1040,2,2000,529,525,3,3000,3020,1040,526,530,600,601,602,603,604,605,606,607,608,609,610,611,612,613,614,615,616,617,618,619,620,621,622,623,624,625,626,627,628,629,630,631,632,633,634,635,636,637,638,639,640,641,642,643,644,645,646,647,648,649,650,651,652,653,654,655,656,657,658,659,660,661,662,663,664,665,666,667,668,669,670,671,672,673,674,675,676,677,701,702"),
        new ("__checkbox_edicao.jornal","1,1000,1010,1020,515,521,522,531,535,536,523,532,540,1040,600,601,602,603,612,613,614,615,616,617,618,619,620,621,622,623,624,625,626,627,628,629,630,631,632,633,701"),
        new ("__checkbox_edicao.jornal","2,2000,529,525,604,605,606,607,634,635,636,637,638,639,640,641,642,643,644,645,646,647,648,649,650,651,652,653,654,655,702"),
        new ("__checkbox_edicao.jornal","3,3000,3020,1040,526,530,608,609,610,611,656,657,658,659,660,661,662,663,664,665,666,667,668,669,670,671,672,673,674,675,676,677"),
        new ("edicao.dtInicio",$"{date:dd/MM}"),
        new ("edicao.dtFim",$"{date:dd/MM}"),
        new ("edicao.ano",$"{date:yyyy}")
    ];

    private static string RemoveQueryString(string url)
        => url[..url.IndexOf('?')];
}