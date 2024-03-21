using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Press.Core.Infrastructure.Scrapers;

namespace Press.Hosts.WebAPI.Tests;

public class PressApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services 
            => services.RemoveAll(typeof(IPublicationProvider)));
    }
}