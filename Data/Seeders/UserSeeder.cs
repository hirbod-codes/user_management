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

        List<User> users = new() { };
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

            users.Add(await CreateUserAsync(superUserId, dt, users));
            dt = dt.AddMinutes(10);
        }

        users.ForEach(user =>
            {
                Faker faker = new Faker("en");
                UserPrivileges userPrivileges = User.GetDefaultUserPrivileges((ObjectId)user.Id!);

                List<Reader> readers = userPrivileges.Readers.ToList();
                List<Updater> updaters = userPrivileges.Updaters.ToList();
                List<Deleter> deleters = userPrivileges.Deleters.ToList();
                Field[] readableFields = User.GetDefaultReadableFields().ToArray();
                Field[] updatableFields = User.GetDefaultUpdatableFields().ToArray();

                if (readableFields.Length == 0)
                    throw new Exception();
                if (updatableFields.Length == 0)
                    throw new Exception();

                readableFields = faker.PickRandom<Field>(items: readableFields, amountToPick: faker.Random.Int(1, readableFields.Length)).ToArray();
                updatableFields = faker.PickRandom<Field>(items: updatableFields, amountToPick: faker.Random.Int(1, updatableFields.Length)).ToArray();

                faker.PickRandom<User>(users, faker.Random.Int(1, users.Count)).ToList().ForEach(u => readers.Add(new Reader() { Author = Reader.USER, AuthorId = (ObjectId)u.Id!, IsPermitted = true, Fields = readableFields }));
                faker.PickRandom<User>(users, faker.Random.Int(1, users.Count)).ToList().ForEach(u => updaters.Add(new Updater() { Author = Updater.USER, AuthorId = (ObjectId)u.Id!, IsPermitted = true, Fields = updatableFields }));
                faker.PickRandom<User>(users, faker.Random.Int(1, users.Count)).ToList().ForEach(u => deleters.Add(new Deleter() { Author = Deleter.USER, AuthorId = (ObjectId)u.Id!, IsPermitted = true }));

                userPrivileges.Readers = readers.ToArray();
                userPrivileges.Updaters = updaters.ToArray();
                userPrivileges.Deleters = deleters.ToArray();

                user.UserPrivileges = userPrivileges;

                _userCollection.ReplaceOne(Builders<User>.Filter.Eq("_id", user.Id), user);
            }
        );

        System.Console.WriteLine("Seeded Users...");
    }
    private async Task<User> CreateUserAsync(ObjectId superUserId, DateTime dt, List<User> users)
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

        UserPrivileges userPrivileges = User.GetDefaultUserPrivileges(userId);

        List<Reader> readers = userPrivileges.Readers.ToList();
        List<Updater> updaters = userPrivileges.Updaters.ToList();
        List<Deleter> deleters = userPrivileges.Deleters.ToList();
        Field[] readableFields = User.GetDefaultReadableFields().ToArray();
        Field[] updatableFields = User.GetDefaultUpdatableFields().ToArray();
        if (readableFields.Length == 0)
            throw new Exception();
        if (updatableFields.Length == 0)
            throw new Exception();

        readableFields = faker.PickRandom<Field>(items: readableFields, amountToPick: faker.Random.Int(1, readableFields.Length)).ToArray();
        updatableFields = faker.PickRandom<Field>(items: updatableFields, amountToPick: faker.Random.Int(1, updatableFields.Length)).ToArray();

        readers.Add(new Reader() { Author = Reader.USER, AuthorId = superUserId, IsPermitted = true, Fields = readableFields });
        updaters.Add(new Updater() { Author = Updater.USER, AuthorId = superUserId, IsPermitted = true, Fields = updatableFields });
        deleters.Add(new Deleter() { Author = Deleter.USER, AuthorId = superUserId, IsPermitted = true });

        userPrivileges.Readers = readers.ToArray();
        userPrivileges.Updaters = updaters.ToArray();
        userPrivileges.Deleters = deleters.ToArray();

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

        return user;
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
    {
        Faker faker = new Faker("en");
        FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;

        TokenPrivileges tokenPrivileges = new TokenPrivileges();

        tokenPrivileges.ReadsFields = User.GetFields().ToArray();
        tokenPrivileges.UpdatesFields = User.GetFields().ToArray();
        tokenPrivileges.DeletesUser = true;
        tokenPrivileges.ReadsFields = User.GetFields().ToArray();

        string codeVerifier = (new StringHelper()).GenerateRandomString(128);

        string code, refreshTokenValue, tokenValue;

        do
        {
            code = (new StringHelper()).GenerateRandomString(128);
            refreshTokenValue = (new StringHelper()).GenerateRandomString(128);
            tokenValue = (new StringHelper()).GenerateRandomString(128);
        } while ((await _userCollection.FindAsync(
            filterBuilder.Or(
                filterBuilder.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, code),
                filterBuilder.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.VALUE, refreshTokenValue),
                filterBuilder.Eq(User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE, (new StringHelper()).HashWithoutSalt(tokenValue))
            ))).FirstOrDefault<User?>() != null);

        ObjectId clientId = (ObjectId)faker.PickRandom<Client>(_clients).Id!;

        UserClient userClient = new UserClient()
        {
            ClientId = clientId,
            RefreshToken = new RefreshToken()
            {
                TokenPrivileges = tokenPrivileges,
                Code = (new StringHelper()).GenerateRandomString(128),
                CodeChallenge = (new StringHelper()).Base64Encode((new StringHelper()).HashWithoutSalt(codeVerifier, "SHA512")!),
                CodeExpiresAt = codeCreateAt,
                CodeChallengeMethod = "SHA512",
                ExpirationDate = exp,
                Value = refreshTokenValue,
                IsVerified = true
            },
            Token = new Token()
            {
                ExpirationDate = exp,
                IsRevoked = false,
                Value = (new StringHelper()).HashWithoutSalt(tokenValue)
            }
        };

        List<UserClient> clients = user.Clients.ToList();
        clients.Add(userClient);
        user.Clients = clients.ToArray();

        if (tokenPrivileges.ReadsFields.Length > 0)
        {
            List<Reader> readers = user.UserPrivileges!.Readers.ToList();
            if (readers.Count != 0)
            {
                Reader? reader = readers.FirstOrDefault<Reader?>(r => r != null && r.Author == Reader.CLIENT && r.AuthorId == clientId, null);
                if (reader != null)
                    readers.Remove(reader);
            }
            readers.Add(new Reader() { AuthorId = clientId, Author = Reader.CLIENT, IsPermitted = true, Fields = tokenPrivileges.ReadsFields });
            user.UserPrivileges!.Readers = readers.ToArray();
        }

        if (tokenPrivileges.UpdatesFields.Length > 0)
        {
            List<Updater> updaters = user.UserPrivileges!.Updaters.ToList();
            if (updaters.Count != 0)
            {
                Updater? updater = updaters.FirstOrDefault<Updater?>(r => r != null && r.Author == Reader.CLIENT && r.AuthorId == clientId, null);
                if (updater != null)
                    updaters.Remove(updater);
            }
            updaters.Add(new Updater() { AuthorId = clientId, Author = Reader.CLIENT, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields });
            user.UserPrivileges!.Updaters = updaters.ToArray();
        }

        if (tokenPrivileges.DeletesUser)
        {
            List<Deleter> deleters = user.UserPrivileges!.Deleters.ToList();
            if (deleters.Count != 0)
            {
                Deleter? deleter = deleters.FirstOrDefault<Deleter?>(r => r != null && r.Author == Reader.CLIENT && r.AuthorId == clientId, null);
                if (deleter != null)
                    deleters.Remove(deleter);
            }
            deleters.Add(new Deleter() { AuthorId = clientId, Author = Reader.CLIENT, IsPermitted = true });
            user.UserPrivileges!.Deleters = deleters.ToArray();
        }

        await File.AppendAllTextAsync(_filePath, @$"
UserId ==> {(ObjectId)user.Id!}
Code verifier ==> {codeVerifier}
Refresh Token value ==> {refreshTokenValue}
Token value ==> {tokenValue}

");
        return user;
    }
}