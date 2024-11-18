namespace Press.Infrastructure.Scrapers.Extractors;

public class DisposableDirectory : IDisposable
{
    private readonly DirectoryInfo _directory;

    public DisposableDirectory() 
        => _directory = Directory.CreateDirectory(GetRandomTempPath());

    public string Path => _directory.FullName;
    
    public void Dispose()
    {
        if (Directory.Exists(_directory.FullName))
            Directory.Delete(_directory.FullName, true);
    }
    
    private string GetRandomTempPath()
        => System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), 
            System.IO.Path.GetRandomFileName());
}