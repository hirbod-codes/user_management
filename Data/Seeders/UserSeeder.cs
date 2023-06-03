namespace user_management.Data.Seeders;

using user_management.Models;
using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Utilities;

public class UserSeeder
{
    private readonly IMongoCollection<Client> _clientCollection;
    private readonly List<Client> _clients = null!;
    private readonly IMongoCollection<User> _userCollection;
    private readonly string _filePath;
    public UserSeeder(MongoContext mongoContext, string rootPath)
    {
        MongoClient mongoClient = new MongoClient(mongoContext.ConnectionString);
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.DatabaseName);
        _clientCollection = mongoDatabase.GetCollection<Client>(mongoContext.Collections.Clients);

        _clients = _clientCollection.Find(Builders<Client>.Filter.Empty).ToList();
        _userCollection = mongoDatabase.GetCollection<User>(mongoContext.Collections.Users);

        var directoryPath = Path.Combine(rootPath, "Data\\Seeders\\Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "user_seeder_logs.log");
    }
    public async Task Seed(int count = 10)
    {
        await File.WriteAllTextAsync(_filePath, "");
        System.Console.WriteLine("Seeding Users...");

        ObjectId superUserId = ObjectId.GenerateNewId();
        DateTime dt = DateTime.UtcNow;
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                await CreateSuperUserAsync(superUserId, dt);
                dt = dt.AddMinutes(10);
                continue;
            }

            await CreateUserAsync(superUserId, dt);
            dt = dt.AddMinutes(10);
        }

        System.Console.WriteLine("Seeded Users...");
    }
    private async Task CreateUserAsync(ObjectId superUserId, DateTime dt)
    {
        Faker faker = new Faker("en");
        ObjectId userId = ObjectId.GenerateNewId();
        FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;
        string email, firstName, middleName, lastName = null!;

        do
        {
            email = faker.Internet.Email();
        } while ((await _userCollection.FindAsync(filterBuilder.Eq(User.EMAIL, email))).FirstOrDefault<User?>() != null);

        do
        {
            firstName = faker.Name.FirstName();
            middleName = faker.Name.FirstName();
            lastName = faker.Name.LastName();
        } while ((await _userCollection.FindAsync(
            filterBuilder.And(
                filterBuilder.Eq(User.FIRST_NAME, firstName),
                filterBuilder.Eq(User.MIDDLE_NAME, middleName),
                filterBuilder.Eq(User.LAST_NAME, lastName)
                )
        )).FirstOrDefault<User?>() != null);

        User user = new User()
        {
            Id = userId,
            UserPrivileges = User.GetDefaultUserPrivileges(userId),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = email,
            PhoneNumber = faker.Person.Phone,
            Username = faker.Person.UserName,
            Password = (new StringHelper()).Hash("password"),
            VerificationSecret = (new StringHelper()).GenerateRandomString(6),
            VerificationSecretUpdatedAt = dt,
            LoggedOutAt = null,
            IsVerified = false,
            CreatedAt = dt,
            UpdatedAt = dt
        };
        user = await AddClient(user, dt.AddMonths(6), dt);
        _userCollection.InsertOne(user);
    }
    private async Task CreateSuperUserAsync(ObjectId userId, DateTime dt)
    {
        Faker faker = new Faker("en");
        FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;
        string firstName, middleName, lastName = null!;

        do
        {
            firstName = faker.Name.FirstName();
            middleName = faker.Name.FirstName();
            lastName = faker.Name.LastName();
        } while ((await _userCollection.FindAsync(
            filterBuilder.And(
                filterBuilder.Eq(User.FIRST_NAME, firstName),
                filterBuilder.Eq(User.MIDDLE_NAME, middleName),
                filterBuilder.Eq(User.LAST_NAME, lastName)
                )
        )).FirstOrDefault<User?>() != null);

        User user = new User()
        {
            Id = userId,
            UserPrivileges = User.GetDefaultUserPrivileges(userId),
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = "taghalloby@gmail.com",
            PhoneNumber = "09380978577",
            Username = "hirbod",
            Password = (new StringHelper()).Hash("password"),
            VerificationSecret = (new StringHelper()).GenerateRandomString(6),
            VerificationSecretUpdatedAt = dt,
            LoggedOutAt = null,
            IsVerified = false,
            CreatedAt = dt,
            UpdatedAt = dt
        };
        user = await AddClient(user, dt.AddMonths(6), dt, true);
        _userCollection.InsertOne(user);
    }
    private async Task<User> AddClient(User user, DateTime exp, DateTime codeCreateAt, bool isSuperUser = false)
