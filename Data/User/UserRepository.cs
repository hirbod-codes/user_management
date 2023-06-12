namespace user_management.Data.User;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;
using user_management.Data;
using MongoDB.Bson;
using user_management.Data.Logics.Filter;
using user_management.Data.Logics.Update;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;

    public UserRepository(IOptions<MongoContext> MongoContext)
    {
        MongoClient mongoClient = new MongoClient(MongoContext.Value.ConnectionString);

        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoContext.Value.DatabaseName);

        _userCollection = mongoDatabase.GetCollection<User>(MongoContext.Value.Collections.Users);
    }

    public async Task<User?> Create(User user)
    {
        user.Id = ObjectId.GenerateNewId();

        DateTime dt = DateTime.UtcNow;
        user.UpdatedAt = dt;
        user.CreatedAt = dt;

        user.UserPrivileges = User.GetDefaultUserPrivileges((ObjectId)user.Id);

        await _userCollection.InsertOneAsync(user);

        return user;
    }

    public async Task<User?> RetrieveByFullNameForExistenceCheck(string fullName) => fullName.Split("-").Length != 3 ? null : (await _userCollection.FindAsync(
        Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(User.FIRST_NAME, fullName.Split("-")[0]),
            Builders<User>.Filter.Eq(User.MIDDLE_NAME, fullName.Split("-")[1]),
            Builders<User>.Filter.Eq(User.LAST_NAME, fullName.Split("-")[2])
        ))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByUsernameForExistenceCheck(string username) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.USERNAME, username))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByEmailForExistenceCheck(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.PHONE_NUMBER, phoneNumber))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq("_id", id), GetReaderFilterDefinition(actorId, forClients)))).FirstOrDefault<User?>();

    public async Task<List<User>> Retrieve(ObjectId actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false)
    {
        if (limit <= 0)
            limit = 5;

        IFilterLogic<User> iLogic = FilterLogics<User>.BuildILogic(logicsString);
        FilterDefinition<User> filter = iLogic.BuildDefinition();
        List<string> requiredFilterFieldsList = iLogic.GetRequiredFields();
        List<string> optionalFilterFieldsList = iLogic.GetOptionalFields();

        List<Field> requiredFilterFields = requiredFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        List<Field> optionalFilterFields = optionalFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        FilterDefinition<User> readPrivilegeFilter = GetReaderFilterDefinition(actorId, forClients, requiredFilterFields, optionalFilterFields);

        FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;
        SortDefinitionBuilder<User> sortBuilder = Builders<User>.Sort;
        SortDefinition<User> sort;
        if (ascending)
            sort = sortBuilder.Ascending(sortBy ?? User.UPDATED_AT);
        else
            sort = sortBuilder.Ascending(sortBy ?? User.UPDATED_AT);

        AggregateFacet<User, AggregateCountResult> countFacet = AggregateFacet.Create("count",
            PipelineDefinition<User, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<User>()
            })
        );

        AggregateFacet<User> dataFacet = AggregateFacet.Create("data",
        PipelineDefinition<User, User>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Skip<User>(iteration * limit),
                PipelineStageDefinitionBuilder.Limit<User>(limit),
            })
        );

        List<AggregateFacetResults> aggregation = await _userCollection.Aggregate()
            .Match(filterBuilder.And(filter, readPrivilegeFilter))
            .Sort(sort)
            .Facet(countFacet, dataFacet)
            .ToListAsync();

        long count = aggregation.First().Facets.First(x => x.Name == "count").Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

        int totalIterations = (int)Math.Ceiling((double)count / iteration);

        return aggregation
            .First()
            .Facets
            .First(x => x.Name == "data")
            .Output<User>()
            .ToList();
    }

    public async Task<User?> RetrieveByIdForAuthentication(ObjectId userId) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.IS_VERIFIED, true), Builders<User>.Filter.Eq<DateTime?>(User.LOGGED_OUT_AT, null), Builders<User>.Filter.Eq("_id", userId)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByIdForAuthorization(ObjectId id) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", id))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserByLoginCredentials(string? email, string? username) => (await _userCollection.FindAsync(Builders<User>.Filter.Or(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Filter.Eq(User.USERNAME, username)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPasswordChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByClientIdAndCode(ObjectId clientId, string code) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.CLIENT_ID, clientId), Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, code)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByRefreshTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.VALUE, value))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE, value))).FirstOrDefault<User?>();

    public async Task<bool?> Login(User user)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", user.Id), Builders<User>.Update.Set<DateTime?>(User.LOGGED_OUT_AT, null).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.IS_VERIFIED, false), Builders<User>.Filter.Eq(User.EMAIL, email)), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecretForPasswordChange(string VerificationSecret, string email)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> Verify(ObjectId id)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", id), Builders<User>.Update.Set<bool>(User.IS_VERIFIED, true).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangePassword(string email, string hashedPassword)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.PASSWORD, hashedPassword).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> Logout(ObjectId id)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", id), Builders<User>.Update.Set<DateTime>(User.LOGGED_OUT_AT, DateTime.UtcNow).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> RemoveClient(User user, ObjectId clientId)
    {
        List<UserClient> userClients = user.Clients.ToList();
        UserClient? userClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == clientId, null);
        if (userClient == null)
            return true;

        userClients.Remove(userClient);

        ReplaceOneResult result = await _userCollection.ReplaceOneAsync(Builders<User>.Filter.Eq("_id", (ObjectId)user.Id!), user);

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> RemoveAllClients(User user)
    {
        user.Clients = new UserClient[] { };

        ReplaceOneResult result = await _userCollection.ReplaceOneAsync(Builders<User>.Filter.Eq("_id", (ObjectId)user.Id!), user);

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> AddToken(User user, ObjectId clientId, string tokenValue, DateTime expirationDate, IClientSessionHandle? session = null)
    {
        List<UserClient> userClients = user.Clients.ToList();
        UserClient? userClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == clientId);
        if (userClient == null)
            return false;

        userClient.Token = new Token() { Value = tokenValue, ExpirationDate = expirationDate, IsRevoked = false };
        userClient.RefreshToken!.IsVerified = true;
        userClient.RefreshToken!.Code = null;

        ReplaceOneResult result;
        if (session != null)
            result = await _userCollection.ReplaceOneAsync(session, Builders<User>.Filter.Eq("_id", (ObjectId)user.Id!), user);
        else
            result = await _userCollection.ReplaceOneAsync(Builders<User>.Filter.Eq("_id", (ObjectId)user.Id!), user);

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> AddTokenPrivileges(User user, ObjectId clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null)
    {
        if (tokenPrivileges.ReadsFields.Length > 0)
        {
            List<Reader> readers = user.UserPrivileges!.Readers.ToList();
            readers.RemoveAll(r => r != null && r.Author == Reader.CLIENT && r.AuthorId == clientId);
            readers.Add(new Reader() { AuthorId = clientId, Author = Reader.CLIENT, IsPermitted = true, Fields = tokenPrivileges.ReadsFields });
            user.UserPrivileges!.Readers = readers.ToArray();
        }

        if (tokenPrivileges.UpdatesFields.Length > 0)
        {
            List<Updater> updaters = user.UserPrivileges!.Updaters.ToList();
            updaters.RemoveAll(u => u != null && u.Author == Updater.CLIENT && u.AuthorId == clientId);
            updaters.Add(new Updater() { AuthorId = clientId, Author = Updater.CLIENT, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields });
            user.UserPrivileges!.Updaters = updaters.ToArray();
        }

        if (tokenPrivileges.DeletesUser)
        {
            List<Deleter> deleters = user.UserPrivileges!.Deleters.ToList();
            deleters.RemoveAll(d => d != null && d.Author == Deleter.CLIENT && d.AuthorId == clientId);
            deleters.Add(new Deleter() { AuthorId = clientId, Author = Deleter.CLIENT, IsPermitted = true });
            user.UserPrivileges!.Deleters = deleters.ToArray();
        }

        user.UpdatedAt = DateTime.UtcNow;

        ReplaceOneResult r;
        if (session != null) r = await _userCollection.ReplaceOneAsync(session, Builders<User>.Filter.Eq<ObjectId>("_id", (ObjectId)user.Id!), user);
        else r = await _userCollection.ReplaceOneAsync(session, Builders<User>.Filter.Eq<ObjectId>("_id", (ObjectId)user.Id!), user);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> AddClientById(User user, ObjectId clientId, ObjectId actorId, bool forClients, TokenPrivileges tokenPrivileges, DateTime refreshTokenExpiration, string refreshTokenValue, DateTime codeExpiresAt, string code, string codeChallenge, string codeChallengeMethod)
    {
        List<UserClient> userClients = user.Clients.ToList();
        UserClient? removableUerClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == clientId, null);
        if (removableUerClient != null)
            userClients.Remove(removableUerClient);

        UserClient userClient = new UserClient()
        {
            ClientId = clientId,
            RefreshToken = new RefreshToken()
            {
                TokenPrivileges = tokenPrivileges,
                Code = code,
                CodeExpiresAt = codeExpiresAt,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                ExpirationDate = refreshTokenExpiration,
                Value = refreshTokenValue,
                IsVerified = false
            },
            Token = null
        };
        userClients.Add(userClient);
        user.Clients = userClients.ToArray();
        user.UpdatedAt = DateTime.UtcNow;

        FilterDefinition<User> filters = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", (ObjectId)user.Id!),
            GetReaderFilterDefinition(actorId, forClients, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } }),
            GetUpdaterFilterDefinition(actorId, forClients, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } })
        );

        ReplaceOneResult r = await _userCollection.ReplaceOneAsync(filters, user);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> UpdateUserPrivileges(ObjectId actorId, ObjectId userId, UserPrivileges userPrivileges, bool forClients)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", userId),
            GetReaderFilterDefinition(actorId, forClients, new List<Field>() { new Field() { IsPermitted = true, Name = User.USER_PRIVILEGES } })
        );
        UpdateDefinition<User> update = Builders<User>.Update.Set(User.USER_PRIVILEGES, userPrivileges).Set(User.UPDATED_AT, DateTime.UtcNow);

        UpdateResult r = await _userCollection.UpdateOneAsync(filter, update);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> Update(ObjectId actorId, string filtersString, string updatesString, bool forClients = false)
    {
        List<FilterDefinition<User>>? filters = new List<FilterDefinition<User>>();

        IFilterLogic<User> iLogic = FilterLogics<User>.BuildILogic(filtersString);
        filters.Add(iLogic.BuildDefinition());

        List<string> requiredFilterFieldsList = iLogic.GetRequiredFields();
        List<string> optionalFilterFieldsList = iLogic.GetOptionalFields();
        List<Field> requiredFilterFields = requiredFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        List<Field> optionalFilterFields = optionalFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });

        filters.Add(GetReaderFilterDefinition(actorId, forClients, requiredFilterFields, optionalFilterFields));

        UpdateLogics<User> logic = new UpdateLogics<User>();
        UpdateDefinition<User>? updates = logic.BuildILogic(updatesString).BuildDefinition().Set(User.UPDATED_AT, DateTime.UtcNow);
        List<Field> updateFieldsList = logic.Fields.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        foreach (Field field in User.GetProtectedFieldsAgainstMassUpdating())
            if (updateFieldsList.FirstOrDefault<Field?>(f => f != null && f.Name == field.Name, null) == null)
                return false;

        filters.Add(GetUpdaterFilterDefinition(actorId, forClients, updateFieldsList));

        UpdateResult result = await _userCollection.UpdateManyAsync(Builders<User>.Filter.And(filters.ToArray()), updates);

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount > 0 && result.ModifiedCount > 0;
    }

    public async Task<bool?> Delete(ObjectId actorId, ObjectId id, bool forClients = false)
    {
        DeleteResult r = await _userCollection.DeleteOneAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq("_id", id), GetDeleterFilterDefinition(actorId, forClients)));

        if (r.IsAcknowledged && r.DeletedCount == 0) return null;

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    private FilterDefinition<User> GetReaderFilterDefinition(ObjectId id, bool isClient, List<Field>? requiredFields = null, List<Field>? optionalFields = null)
    {
        FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
        List<FilterDefinition<User>> filters = new() {
            builder.Eq(Reader.AUTHOR, isClient ? Reader.CLIENT : Reader.USER),
            builder.Eq(Reader.AUTHOR_ID, id),
            builder.Eq(Reader.IS_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            filters.Add(builder.All(Reader.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            filters.Add(builder.In(Reader.FIELDS, optionalFields));

        List<FilterDefinition<User>> allFilters = new() {
            builder.Eq(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.ARE_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            allFilters.Add(builder.All(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            allFilters.Add(builder.In(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.FIELDS, optionalFields));

        return builder.Or(
                    builder.And(
                        builder.SizeGt(User.USER_PRIVILEGES + "." + UserPrivileges.READERS, 0),
                        builder.ElemMatch(User.USER_PRIVILEGES + "." + UserPrivileges.READERS, builder.And(filters))
                    ),
                    builder.And(allFilters)
                );
    }

    private FilterDefinition<User> GetUpdaterFilterDefinition(ObjectId id, bool isClient, List<Field>? fields = null) => Builders<User>.Filter.Or(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.SizeGt(User.USER_PRIVILEGES + "." + UserPrivileges.UPDATERS, 0),
                        Builders<User>.Filter.ElemMatch(User.USER_PRIVILEGES + "." + UserPrivileges.UPDATERS,
                        (fields == null || fields.Count == 0) ?
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, id),
                            Builders<User>.Filter.Eq(Updater.IS_PERMITTED, true)
                        ) :
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, id),
                            Builders<User>.Filter.Eq(Updater.IS_PERMITTED, true),
                            Builders<User>.Filter.All(Updater.FIELDS, fields)
                        ))
                    ),
                    (fields == null || fields.Count == 0) ?
                    Builders<User>.Filter.Eq(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_UPDATERS + "." + AllUpdaters.ARE_PERMITTED, true) :
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_UPDATERS + "." + AllUpdaters.ARE_PERMITTED, true),
                        Builders<User>.Filter.All(User.USER_PRIVILEGES + "." + UserPrivileges.ALL_UPDATERS + "." + AllUpdaters.FIELDS, fields)
                    )
                );

    private FilterDefinition<User> GetDeleterFilterDefinition(ObjectId id, bool isClient) => Builders<User>.Filter.And(
                    Builders<User>.Filter.SizeGt(User.USER_PRIVILEGES + "." + UserPrivileges.DELETERS, 0),
                    Builders<User>.Filter.ElemMatch(User.USER_PRIVILEGES + "." + UserPrivileges.DELETERS,
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Deleter.AUTHOR, isClient ? Deleter.CLIENT : Deleter.USER),
                            Builders<User>.Filter.Eq(Deleter.AUTHOR_ID, id),
                            Builders<User>.Filter.Eq(Deleter.IS_PERMITTED, true)
                        )
                    )
                );
}