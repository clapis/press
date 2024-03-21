using System.Net.Http.Json;
using Press.Core.Domain;

namespace Press.Hosts.WebAPI.Tests;

public class EndpointTests(PressApplicationFactory factory) 
    : IClassFixture<PressApplicationFactory>
{
    [Fact(DisplayName = "Healthcheck endpoint returns healthy")]
    public async Task Test01()
    {
        using var client = factory.CreateClient();

        var contents = await client.GetStringAsync("/healthz");
        
        Assert.Equal("Healthy", contents);
    }

    [Fact(DisplayName = "Returns latest publications by source")]
    public async Task Test02()
    {
        using var client = factory.CreateClient();

        var publications = await client.GetFromJsonAsync<List<Publication>>("/publications/latest-by-source");
        
        Assert.Empty(publications!);
    }
}