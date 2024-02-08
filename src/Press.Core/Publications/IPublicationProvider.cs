namespace Press.Core.Publications
{
    public interface IPublicationProvider
    {
        IAsyncEnumerable<Publication> ProvideAsync(CancellationToken cancellationToken);
    }
}