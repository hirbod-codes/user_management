using System.Reflection.Metadata;
namespace user_management.Data.User;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;
using user_management.Services.Data.User;
using MongoDB.Bson;
using user_management.Data.Logics.Filter;
using user_management.Data.Logics.Update;
using user_management.Services.Data;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<PartialUser> _partialUserCollection;

    public UserRepository(IOptions<MongoContext> mongoContext)
    {
        MongoClient mongoClient = MongoContext.GetMongoClient(mongoContext.Value);

        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.Value.DatabaseName);

        _userCollection = mongoDatabase.GetCollection<User>(mongoContext.Value.Collections.Users);
        _partialUserCollection = mongoDatabase.GetCollection<PartialUser>(mongoContext.Value.Collections.Users);
    }

    public async Task<User?> Create(User user)
    {
        user.Id = ObjectId.GenerateNewId();

        DateTime dt = DateTime.UtcNow;
        user.UpdatedAt = dt;
        user.CreatedAt = dt;

        try { await _userCollection.InsertOneAsync(user); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return user;
    }

    public async Task<User?> RetrieveByFullNameForExistenceCheck(string firstName, string middleName, string lastName) => (await _userCollection.FindAsync(
        Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(User.FIRST_NAME, firstName),
            Builders<User>.Filter.Eq(User.MIDDLE_NAME, middleName),
            Builders<User>.Filter.Eq(User.LAST_NAME, lastName)
        ))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByUsernameForExistenceCheck(string username) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.USERNAME, username))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByEmailForExistenceCheck(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.PHONE_NUMBER, phoneNumber))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveById(ObjectId id) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", id))).FirstOrDefault<User?>();

    public async Task<PartialUser?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false) => (await _partialUserCollection.FindAsync(Builders<PartialUser>.Filter.And(Builders<PartialUser>.Filter.Eq("_id", id), GetReaderFilterDefinitionForReads(actorId, forClients)))).FirstOrDefault<PartialUser?>();

    public async Task<List<PartialUser>> Retrieve(ObjectId actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false)
    {
        if (limit <= 0) limit = 5;

        FilterDefinition<PartialUser> filter = Builders<PartialUser>.Filter.Empty;
        List<string> requiredFilterFieldsList = new List<string> { };
        List<string> optionalFilterFieldsList = new List<string> { };
        List<Field> requiredFilterFields = new List<Field> { };
        List<Field> optionalFilterFields = new List<Field> { };
        if (logicsString != "empty")
        {
            IFilterLogic<PartialUser> iLogic = FilterLogics<PartialUser>.BuildILogic(logicsString);
            filter = iLogic.BuildDefinition();
            requiredFilterFieldsList = iLogic.GetRequiredFields();
            optionalFilterFieldsList = iLogic.GetOptionalFields();
            requiredFilterFields = requiredFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
            optionalFilterFields = optionalFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        }

        FilterDefinition<PartialUser> readPrivilegeFilter = GetReaderFilterDefinitionForReads(actorId, forClients, requiredFilterFields.Count == 0 ? null : requiredFilterFields, optionalFilterFields.Count == 0 ? null : optionalFilterFields);

        FilterDefinitionBuilder<PartialUser> filterBuilder = Builders<PartialUser>.Filter;
        SortDefinitionBuilder<PartialUser> sortBuilder = Builders<PartialUser>.Sort;
        SortDefinition<PartialUser> sort;
        if (ascending)
            sort = sortBuilder.Ascending(sortBy ?? PartialUser.UPDATED_AT);
        else
            sort = sortBuilder.Ascending(sortBy ?? PartialUser.UPDATED_AT);

        AggregateFacet<PartialUser, AggregateCountResult> countFacet = AggregateFacet.Create("count",
            PipelineDefinition<PartialUser, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<PartialUser>()
            })
        );

        AggregateFacet<PartialUser> dataFacet = AggregateFacet.Create("data",
        PipelineDefinition<PartialUser, PartialUser>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Skip<PartialUser>(iteration * limit),
                PipelineStageDefinitionBuilder.Limit<PartialUser>(limit),
            })
        );

        List<AggregateFacetResults> aggregation = await _partialUserCollection.Aggregate()
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
            .Output<PartialUser>()
            .ToList();
    }

    public async Task<User?> RetrieveByIdForAuthentication(ObjectId userId) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.IS_VERIFIED, true), Builders<User>.Filter.Eq<DateTime?>(User.LOGGED_OUT_AT, null), Builders<User>.Filter.Eq("_id", userId)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByIdForAuthorization(ObjectId id) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", id))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserByLoginCredentials(string? email, string? username) => (await _userCollection.FindAsync(Builders<User>.Filter.Or(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Filter.Eq(User.USERNAME, username)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPasswordChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForUsernameChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForEmailChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPhoneNumberChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByClientIdAndCode(ObjectId clientId, string code) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.CLIENT_ID, clientId), Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, code)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByRefreshTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.VALUE, value))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE, value))).FirstOrDefault<User?>();

    public async Task<bool?> Login(ObjectId userId)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", userId), Builders<User>.Update.Set<DateTime?>(User.LOGGED_OUT_AT, null).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecretForActivation(string VerificationSecret, string email)
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

    public async Task<bool?> ChangeUsername(string email, string username)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.USERNAME, username).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangePhoneNumber(string email, string phoneNumber)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.PHONE_NUMBER, phoneNumber).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangeEmail(string email, string newEmail)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.EMAIL, newEmail).Set(User.UPDATED_AT, DateTime.UtcNow));

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

    public async Task<bool?> RemoveClient(ObjectId userId, ObjectId clientId, ObjectId authorId, bool isClient)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(
            Builders<User>.Filter.And(
                GetUpdaterFilterDefinition(authorId, isClient, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } }),
                Builders<User>.Filter.Eq("_id", userId)
            ),
            Builders<User>.Update
                .Pull<ObjectId>(User.CLIENTS + "." + UserClient.CLIENT_ID, clientId)
                .Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow)
        );

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> RemoveAllClients(ObjectId userId, ObjectId authorId, bool isClient)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(
            Builders<User>.Filter.And(
                GetUpdaterFilterDefinition(authorId, isClient, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } }),
                Builders<User>.Filter.Eq("_id", userId)
            ),
            Builders<User>.Update
                .Set<UserClient[]>(User.CLIENTS, new UserClient[] { })
                .Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow)
        );

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> AddToken(ObjectId userId, ObjectId authorId, ObjectId clientId, string tokenValue, DateTime expirationDate, IClientSessionHandle? session = null)
    {
        FilterDefinition<User> filterDefinition = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq<ObjectId>("_id", userId),
            Builders<User>.Filter.Eq<ObjectId>(User.CLIENTS + "." + UserClient.CLIENT_ID, clientId),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.CLIENTS } })
        );

        UpdateDefinition<User> updateDefinition = Builders<User>.Update
            .Set<Token>(User.CLIENTS + "$." + UserClient.TOKEN, new Token() { Value = tokenValue, ExpirationDate = expirationDate, IsRevoked = false })
            .Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow);

        UpdateResult result;
        if (session != null)
            result = await _userCollection.UpdateOneAsync(session, filterDefinition, updateDefinition);
        else
            result = await _userCollection.UpdateOneAsync(filterDefinition, updateDefinition);

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> AddTokenPrivilegesToUser(ObjectId userId, ObjectId authorId, ObjectId clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null)
    {
        FilterDefinition<User> filterDefinition = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq<ObjectId>("_id", userId),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PRIVILEGES } })
        );

        if (tokenPrivileges.ReadsFields.Length == 0 && tokenPrivileges.UpdatesFields.Length == 0 && !tokenPrivileges.DeletesUser) return null;

        UpdateDefinition<User> updateDefinition = Builders<User>.Update.Set<DateTime>(User.UPDATED_AT, DateTime.UtcNow);

        if (tokenPrivileges.ReadsFields.Length > 0)
            updateDefinition = updateDefinition.Push<User, Reader>(User.USER_PRIVILEGES + "." + UserPrivileges.READERS, new Reader() { AuthorId = clientId, Author = Reader.CLIENT, IsPermitted = true, Fields = tokenPrivileges.ReadsFields });
        if (tokenPrivileges.UpdatesFields.Length > 0)
            updateDefinition = updateDefinition.Push<User, Updater>(User.USER_PRIVILEGES + "." + UserPrivileges.UPDATERS, new Updater() { AuthorId = clientId, Author = Updater.CLIENT, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields });
        if (tokenPrivileges.DeletesUser)
            updateDefinition = updateDefinition.Push<User, Deleter>(User.USER_PRIVILEGES + "." + UserPrivileges.DELETERS, new Deleter() { AuthorId = clientId, Author = Deleter.CLIENT, IsPermitted = true });

        UpdateResult r;
        if (session != null) r = await _userCollection.UpdateOneAsync(session, filterDefinition, updateDefinition);
        else r = await _userCollection.UpdateOneAsync(session, filterDefinition, updateDefinition);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> AddClientById(ObjectId userId, ObjectId clientId, ObjectId actorId, TokenPrivileges tokenPrivileges, DateTime refreshTokenExpiration, string refreshTokenValue, DateTime codeExpiresAt, string code, string codeChallenge, string codeChallengeMethod)
    {
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

        FilterDefinition<User> filters = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", userId),
            Builders<User>.Filter.Ne(User.CLIENTS + "." + UserClient.CLIENT_ID, clientId),
            GetReaderFilterDefinition(actorId, false, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } }),
            GetUpdaterFilterDefinition(actorId, false, new List<Field>() { new Field() { IsPermitted = true, Name = User.CLIENTS } })
        );

        UpdateResult r = await _userCollection.UpdateOneAsync(filters, Builders<User>.Update.Push<UserClient>(User.CLIENTS, userClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow));

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> UpdateUserPrivileges(ObjectId authorId, ObjectId userId, UserPrivileges userPrivileges)
    {
        FilterDefinition<User> filters = Builders<User>.Filter.And(
            GetUpdaterFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PRIVILEGES } }),
            Builders<User>.Filter.Eq("_id", authorId)
        );

        UpdateResult r = await _userCollection.UpdateOneAsync(filters, Builders<User>.Update.Set<UserPrivileges>(User.USER_PRIVILEGES, userPrivileges));

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
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
            if (updateFieldsList.FirstOrDefault<Field?>(f => f != null && f.Name == field.Name, null) != null)
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

    private FilterDefinition<PartialUser> GetReaderFilterDefinitionForReads(ObjectId actorId, bool isClient, List<Field>? requiredFields = null, List<Field>? optionalFields = null)
    {
        FilterDefinitionBuilder<PartialUser> builder = Builders<PartialUser>.Filter;
        List<FilterDefinition<PartialUser>> filters = new() {
            builder.Eq(Reader.AUTHOR, isClient ? Reader.CLIENT : Reader.USER),
            builder.Eq(Reader.AUTHOR_ID, actorId),
            builder.Eq(Reader.IS_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            filters.Add(builder.All(Reader.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            filters.Add(builder.In(Reader.FIELDS, optionalFields));

        List<FilterDefinition<PartialUser>> allFilters = new() {
            builder.Eq(PartialUser.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.ARE_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            allFilters.Add(builder.All(PartialUser.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            allFilters.Add(builder.In(PartialUser.USER_PRIVILEGES + "." + UserPrivileges.ALL_READERS + "." + AllReaders.FIELDS, optionalFields));

        return builder.Or(
                    builder.And(
                        builder.SizeGt(PartialUser.USER_PRIVILEGES + "." + UserPrivileges.READERS, 0),
                        builder.ElemMatch(PartialUser.USER_PRIVILEGES + "." + UserPrivileges.READERS, builder.And(filters))
                    ),
                    builder.And(allFilters)
                );
    }

    private FilterDefinition<User> GetReaderFilterDefinition(ObjectId actorId, bool isClient, List<Field>? requiredFields = null, List<Field>? optionalFields = null)
    {
        FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
        List<FilterDefinition<User>> filters = new() {
            builder.Eq(Reader.AUTHOR, isClient ? Reader.CLIENT : Reader.USER),
            builder.Eq(Reader.AUTHOR_ID, actorId),
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

    private FilterDefinition<User> GetUpdaterFilterDefinition(ObjectId actorId, bool isClient, List<Field>? fields = null) => Builders<User>.Filter.Or(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.SizeGt(User.USER_PRIVILEGES + "." + UserPrivileges.UPDATERS, 0),
                        Builders<User>.Filter.ElemMatch(User.USER_PRIVILEGES + "." + UserPrivileges.UPDATERS,
                        (fields == null || fields.Count == 0) ?
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, actorId),
                            Builders<User>.Filter.Eq(Updater.IS_PERMITTED, true)
                        ) :
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, actorId),
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

    private FilterDefinition<User> GetDeleterFilterDefinition(ObjectId actorId, bool isClient) => Builders<User>.Filter.And(
                    Builders<User>.Filter.SizeGt(User.USER_PRIVILEGES + "." + UserPrivileges.DELETERS, 0),
                    Builders<User>.Filter.ElemMatch(User.USER_PRIVILEGES + "." + UserPrivileges.DELETERS,
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Deleter.AUTHOR, isClient ? Deleter.CLIENT : Deleter.USER),
                            Builders<User>.Filter.Eq(Deleter.AUTHOR_ID, actorId),
                            Builders<User>.Filter.Eq(Deleter.IS_PERMITTED, true)
                        )
                    )
                );
}