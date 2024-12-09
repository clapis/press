using Polly;
using Polly.Retry;

namespace Press.Infrastructure.Scrapers.Extensions;

public static class HttpClientExtensions
{
    private static readonly ResiliencePipeline ResiliencePipeline = BuildResiliencePipeline();
    
    public static async Task<DisposableFile> DownloadAsync(this HttpClient client, string url, CancellationToken cancellationToken)
    {
        var file = new DisposableFile();

        await ResiliencePipeline.ExecuteAsync(async ct 
            => await client.DownloadCoreAsync(url, file.Path, ct), cancellationToken);

        return file;
    }
    
    private static async Task DownloadCoreAsync(this HttpClient client, string url, string filepath, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        
        if (File.Exists(filepath))
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(new FileInfo(filepath).Length, null);
            
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        response.EnsureSuccessStatusCode();
        response.EnsureAllowedContentType();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.None, 8192, true);
        
        await stream.CopyToAsync(fs, cancellationToken);
    }

    private static ResiliencePipeline BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                ShouldHandle = new PredicateBuilder().Handle<HttpIOException>(),
            }) 
            .Build(); 
    }
}

public static class HttpResponseMessageExtensions
{
    private static readonly string[] AllowedContentTypes = new[] { "application/pdf", "application/octet-stream" };

    public static void EnsureAllowedContentType(this HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;
        
        if (!AllowedContentTypes.Contains(contentType))
            throw new Exception($"Content-Type '{contentType}' is not expected (request uri: {response.RequestMessage?.RequestUri})");
    }
}

public class DisposableFile : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
    
    public void Dispose()
    {
        if (File.Exists(Path)) 
            File.Delete(Path);
    }
}