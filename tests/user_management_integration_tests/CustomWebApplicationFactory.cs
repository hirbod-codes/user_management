using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using user_management.Utilities;

namespace user_management_integration_tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public Mock<INotificationHelper> INotificationHelper { get; set; } = new Mock<INotificationHelper>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("/home/hirbod/projects/microservice/user_management/src/user_management/appsettings.Development.json")
            .Build();

        builder
            .UseConfiguration(configuration)
            .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton(INotificationHelper.Object);
                })
            .UseEnvironment("Production")
            ;
    }
}
