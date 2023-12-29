using Newtonsoft.Json;
using user_management.Models;

namespace user_management.Data.InMemory.Seeders;

public class UserSeeder : Data.Seeders.UserSeeder
{
    private string? _filePath;
    private InMemoryContext _inMemoryContext = null!;
    public const string USERS_PASSWORDS = "Pass%w0rd!99";

    public UserSeeder(InMemoryContext inMemoryContext, string? rootPath)
    {
        if (rootPath is not null)
            SetFilePath(rootPath);

        _inMemoryContext = inMemoryContext;
    }

    public void SetFilePath(string rootPath)
    {
        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public async Task Seed(int count, FakeUserOptions? fakeUserOptions = null)
    {
        System.Console.WriteLine("\nSeeding Users...");

        IEnumerable<Models.User> users = GenerateClients(count, users: _inMemoryContext.Users, fakeUserOptions);

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await Store(users);

        System.Console.WriteLine("Seeded Users...\n");
    }

    public async Task SeedAdmin(string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        System.Console.WriteLine("\nSeeding Admin User...");

        Models.User? lastUser = _inMemoryContext.Users.LastOrDefault();
        string adminId = lastUser is null ? "1" : (long.Parse(lastUser.Id) + 1).ToString();
        await Store(GetAdminUser(adminId, adminUsername, adminPassword, adminEmail, adminPhoneNumber));

        List<Models.User> users = _inMemoryContext.Users.ToList();

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        users.ForEach(u =>
        {
            if (u.Id == adminId)
                return;
            u.UserPermissions.Readers = u.UserPermissions.Readers.Append(new() { Author = Reader.USER, AuthorId = adminId, IsPermitted = true, Fields = Models.User.GetReadableFields().ToArray() }).ToArray();
            u.UserPermissions.Updaters = u.UserPermissions.Updaters.Append(new() { Author = Updater.USER, AuthorId = adminId, IsPermitted = true, Fields = Models.User.GetUpdatableFields().ToArray() }).ToArray();
            u.UserPermissions.Deleters = u.UserPermissions.Deleters.Append(new() { Author = Deleter.USER, AuthorId = adminId, IsPermitted = true }).ToArray();
        });

        await _inMemoryContext.SaveChangesAsync();

        System.Console.WriteLine("Seeded Admin User...\n");
    }

    private async Task Store(Models.User user)
    {
        await _inMemoryContext.Users.AddAsync(user);
        await _inMemoryContext.SaveChangesAsync();
    }

    private async Task Store(IEnumerable<Models.User> users)
    {
        await _inMemoryContext.Users.AddRangeAsync(users);
        await _inMemoryContext.SaveChangesAsync();
    }

    public IEnumerable<Models.User> GenerateClients(int count = 2, IEnumerable<Models.User>? users = null, FakeUserOptions? fakeUserOptions = null)
    {
        users ??= Array.Empty<Models.User>();
        for (int i = 0; i < count; i++)
        {
            Models.User? lastUser = users.LastOrDefault();
            string userId = lastUser is null ? "1" : (long.Parse(lastUser.Id) + 1).ToString();

            users = users.Append(FakeUser(userId, users: users, clients: _inMemoryContext.Clients, fakeUserOptions, password: USERS_PASSWORDS));

            if ((i + 1) % 10 == 0)
                System.Console.WriteLine($"Generated {i + 1} users\n");
        }
        return users;
    }
}
