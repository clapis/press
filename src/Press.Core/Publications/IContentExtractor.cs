namespace Press.Core.Publications
{
    public interface IContentExtractor
    {
        Task<string> ExtractAsync(string link, CancellationToken cancellationToken);
    }

}