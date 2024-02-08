using System.Threading;
using System.Threading.Tasks;

namespace Press.Core.Publications
{
    public interface IContentExtractor
    {
        Task<string> ExtractAsync(string link, CancellationToken cancellationToken);
    }

}