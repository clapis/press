namespace Press.Core.Publications
{
    public interface IPublicationStore
    {
        Task<List<string>> GetAllUrlsAsync(CancellationToken cancellationToken);
        Task SaveAsync(Publication publication, CancellationToken cancellationToken);
        Task<List<Publication>> SearchAsync(string query, CancellationToken cancellationToken);
        Task<List<Publication>> GetLatestPublicationsBySourceAsync(CancellationToken cancellationToken);
    }
}