using Microsoft.EntityFrameworkCore;
using user_management.Data.InMemory.Client;
using user_management.Data.InMemory.User;
using user_management.Services.Data.Client;
using user_management.Services.Data.User;

namespace user_management.Data.InMemory;

public class InMemoryContext : DbContext
{
    public string DatabaseName { get; set; }
    public DbSet<Models.Privilege> Privileges { get; set; }
    public DbSet<Models.User> Users { get; set; }
    public DbSet<Models.Client> Clients { get; set; }

    public InMemoryContext(string databaseName) => DatabaseName = databaseName;

    public async Task Initialize() => await ClearDatabase();

    public async Task ClearDatabase() => await Database.EnsureDeletedAsync();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(DatabaseName);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Models.Privilege>();
        modelBuilder.Ignore<Models.Reader>();
        modelBuilder.Ignore<Models.AllReaders>();
        modelBuilder.Ignore<Models.Updater>();
        modelBuilder.Ignore<Models.AllUpdaters>();
        modelBuilder.Ignore<Models.Deleter>();
        modelBuilder.Ignore<Models.Field>();
        modelBuilder.Ignore<Models.AuthorizedClient>();
        modelBuilder.Ignore<Models.AuthorizingClient>();
        modelBuilder.Ignore<Models.PartialUser>();
        modelBuilder.Ignore<Models.Token>();
        modelBuilder.Ignore<Models.TokenPrivileges>();
        modelBuilder.Ignore<Models.RefreshToken>();
        modelBuilder.Ignore<Models.UserPermissions>();
    }

    public static void ConfigureInMemory(IServiceCollection services, IConfiguration configuration)
    {
        InMemoryContext dbContext = new(configuration["DB_OPTIONS:DatabaseName"]!);

        // dbContext.Database.EnsureCreated();

        configuration.GetSection("DB_OPTIONS").Bind(dbContext);
        services.AddSingleton(dbContext);

        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IClientRepository, ClientRepository>();
    }
}
