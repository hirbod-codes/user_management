using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using user_management.Utilities;

namespace user_management_integration_tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public Mock<INotificationHelper> INotificationHelper { get; set; } = new Mock<INotificationHelper>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(Program.ENV_PREFIX + "ENV_FILE_PATH", ".env.sharded_mongodb.development");
        Environment.SetEnvironmentVariable("ENV_FILE_PATH", ".env.sharded_mongodb.development");
        builder
            .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton(INotificationHelper.Object);
                })
            ;
    }
}
