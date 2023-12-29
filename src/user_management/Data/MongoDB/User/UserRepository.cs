namespace user_management.Data.MongoDB.User;

using global::MongoDB.Driver;
using user_management.Models;
using user_management.Services.Data.User;
using global::MongoDB.Bson;
using user_management.Services.Data;
using global::MongoDB.Driver.Linq;
using user_management.Data.MongoDB;

public class UserRepository : MongoDBAtomicity, IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<PartialUser> _partialUserCollection;

    public UserRepository(MongoCollections mongoCollections, IMongoClient mongoClient) : base(mongoClient)
    {
        _userCollection = mongoCollections.Users;
        _partialUserCollection = mongoCollections.PartialUsers;
    }

    public string GenerateId() => ObjectId.GenerateNewId().ToString();

    public async Task<User?> Create(User user)
    {
        DateTime dt = DateTime.UtcNow;
        user.UpdatedAt = dt;
        user.CreatedAt = dt;

        try { await _userCollection.InsertOneAsync(user); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        return user;
    }

    public async Task<User?> RetrieveByFullNameForExistenceCheck(string? firstName, string? middleName, string? lastName)
    {
        if (firstName == null && middleName == null && lastName == null) throw new ArgumentException();

        return (await _userCollection.FindAsync(
            Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(User.FIRST_NAME, firstName),
                Builders<User>.Filter.Eq(User.MIDDLE_NAME, middleName),
                Builders<User>.Filter.Eq(User.LAST_NAME, lastName)
            ))
        ).FirstOrDefault<User?>();
    }

    public async Task<User?> RetrieveByUsernameForExistenceCheck(string username) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.USERNAME, username))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByEmailForExistenceCheck(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.PHONE_NUMBER, phoneNumber))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveById(string id) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)))).FirstOrDefault<User?>();

    public async Task<PartialUser?> RetrieveById(string actorId, string id, bool forClients = false)
    {
        return (
            await RetrievePipeline(
                _partialUserCollection.Aggregate()
                .Match(Builders<PartialUser>.Filter.And(Builders<PartialUser>.Filter.Eq("_id", ObjectId.Parse(id)), GetReaderFilterDefinitionForReads(actorId, forClients)))
                .As<BsonDocument>(), actorId
            )
            .As<PartialUser>()
            .ToListAsync()
        )
        .FirstOrDefault<PartialUser>();
    }

    public async Task<List<PartialUser>> Retrieve(string actorId, Data.Logics.Filter? filters, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false)
    {
        if (limit <= 0) limit = 5;

        FilterDefinition<PartialUser> filter = Builders<PartialUser>.Filter.Empty;
        List<Field> requiredFilterFields = new() { };
        if (filters is not null)
        {
            filter = Logics.Filter<PartialUser>.BuildDefinition(filters);
            requiredFilterFields = filters.GetFields().ToList().ConvertAll((f) => new Field() { Name = f, IsPermitted = true });
        }

        FilterDefinition<PartialUser> readPrivilegeFilter = GetReaderFilterDefinitionForReads(actorId, forClients, requiredFilterFields.Any() ? requiredFilterFields : null);

        SortDefinition<PartialUser> sort;
        if (ascending)
            sort = Builders<PartialUser>.Sort.Ascending(sortBy ?? PartialUser.UPDATED_AT);
        else
            sort = Builders<PartialUser>.Sort.Descending(sortBy ?? PartialUser.UPDATED_AT);

        return await RetrievePipeline(
            _partialUserCollection.Aggregate()
            .Match(Builders<PartialUser>.Filter.And(filter, readPrivilegeFilter))
            .Sort(sort)
            .Skip((long)(iteration * limit))
            .Limit((long)limit)
            .As<BsonDocument>(),
            actorId
        )
        .As<PartialUser>()
        .ToListAsync();
    }

    public async Task<User?> RetrieveByIdForAuthenticationHandling(string userId) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.IS_EMAIL_VERIFIED, true), Builders<User>.Filter.Eq<DateTime?>(User.LOGGED_OUT_AT, null), Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId))))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByIdForAuthorizationHandling(string id) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserByLoginCredentials(string? email, string? username) => (await _userCollection.FindAsync(Builders<User>.Filter.Or(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Filter.Eq(User.USERNAME, username)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPasswordChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForUsernameChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForUnverifiedEmailChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Filter.Eq(User.IS_EMAIL_VERIFIED, false)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForEmailChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPhoneNumberChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByClientIdAndCode(string clientId, string code) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.AUTHORIZING_CLIENT + "." + AuthorizingClient.CLIENT_ID, ObjectId.Parse(clientId)), Builders<User>.Filter.Eq(User.AUTHORIZING_CLIENT + "." + AuthorizingClient.CODE, code)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByRefreshTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.REFRESH_TOKEN + "." + RefreshToken.VALUE, value))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.TOKEN + "." + Token.VALUE, value))).FirstOrDefault<User?>();

    public async Task<bool?> Login(string userId)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Set<DateTime?>(User.LOGGED_OUT_AT, null)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount <= 1;
    }

    public async Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecretForActivation(string VerificationSecret, string email)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq(User.IS_EMAIL_VERIFIED, false), Builders<User>.Filter.Eq(User.EMAIL, email)), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateVerificationSecretForPasswordChange(string VerificationSecret, string email)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> Verify(string id)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)), Builders<User>.Update.Set<bool>(User.IS_EMAIL_VERIFIED, true)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangePassword(string email, string hashedPassword)
    {
        UpdateResult result;
        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.PASSWORD, hashedPassword).Set(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangeUsername(string email, string username)
    {
        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.USERNAME, username).Set(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangePhoneNumber(string email, string phoneNumber)
    {
        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.PHONE_NUMBER, phoneNumber).Set(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangeEmail(string email, string newEmail)
    {
        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.EMAIL, newEmail).Set(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> Logout(string id)
    {
        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)), Builders<User>.Update.Set<DateTime>(User.LOGGED_OUT_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> RemoveClient(string userId, string clientId, string authorId, bool isClient)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.And(
            GetReaderFilterDefinition(authorId, isClient, new List<Field>() { new Field() { IsPermitted = true, Name = User.AUTHORIZED_CLIENTS } }),
            GetUpdaterFilterDefinition(authorId, isClient, new List<Field>() { new Field() { IsPermitted = true, Name = User.AUTHORIZED_CLIENTS } }),
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId))
        );

        UpdateDefinition<User> update = Builders<User>.Update
            .PullFilter(x => x.AuthorizedClients, x => x.ClientId == clientId)
            .Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow);

        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(filter, update); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> RemoveAllClients(string userId, string authorId, bool isClient)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.And(
            GetUpdaterFilterDefinition(authorId, isClient, new List<Field>() { new Field() { IsPermitted = true, Name = User.AUTHORIZED_CLIENTS } }),
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId))
        );

        UpdateDefinition<User> update = Builders<User>.Update
            .Set<AuthorizedClient[]>(User.AUTHORIZED_CLIENTS, new AuthorizedClient[] { })
            .Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow);

        UpdateResult result;

        try { result = await _userCollection.UpdateOneAsync(filter, update); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> AddTokenPrivilegesToUserWithTransaction(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges)
    {
        FilterDefinition<User> filterDefinition = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } }),
            GetUpdaterFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } })
        );

        if (tokenPrivileges.ReadsFields.Length == 0 && tokenPrivileges.UpdatesFields.Length == 0 && !tokenPrivileges.DeletesUser) return null;

        UpdateDefinition<User> updateDefinition = Builders<User>.Update.Pipeline(GetAddTokenPrivilegesToUserPipeLine(clientId, tokenPrivileges));

        UpdateResult r = await _userCollection.UpdateOneAsync(_session, filterDefinition, updateDefinition);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> AddTokenPrivilegesToUser(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges)
    {
        FilterDefinition<User> filterDefinition = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } }),
            GetUpdaterFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } })
        );

        if (tokenPrivileges.ReadsFields.Length == 0 && tokenPrivileges.UpdatesFields.Length == 0 && !tokenPrivileges.DeletesUser) return null;

        UpdateDefinition<User> updateDefinition = Builders<User>.Update.Pipeline(GetAddTokenPrivilegesToUserPipeLine(clientId, tokenPrivileges));

        UpdateResult r = await _userCollection.UpdateOneAsync(filterDefinition, updateDefinition);

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    private PipelineDefinition<User, User> GetAddTokenPrivilegesToUserPipeLine(string clientId, TokenPrivileges tokenPrivileges)
    {
        EmptyPipelineDefinition<User> pipelineDefinition = new();
        PipelineDefinition<User, User>? pipeline = null;

        if (tokenPrivileges.ReadsFields.Length > 0)
        {
            string fields = "";
            tokenPrivileges.ReadsFields.ToList().ConvertAll<string>(f => $"{{'{Field.NAME}': '{f.Name}', '{Field.IS_PERMITTED}': {f.IsPermitted.ToString().ToLower()}}}").ToList().ForEach(f => fields += f + ", ");
            fields = "[" + fields + "]";

            Func<PipelineDefinition<User, User>, PipelineDefinition<User, User>> f = (p) => p
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.READERS}': {{
                            $filter: {{
                                'input': '${User.USER_PERMISSIONS}.{UserPermissions.READERS}',
                                'as': 'item',
                                'cond': {{ 
                                    '$ne': [
                                        '$$item.{Reader.AUTHOR_ID}', ObjectId('{clientId}')
                                    ]
                                }}
                            }}
                        }}
                    }}
                }}")
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.READERS}': {{ $concatArrays: [ '${User.USER_PERMISSIONS}.{UserPermissions.READERS}',
                                [
                                    {{
                                        '{Reader.AUTHOR_ID}': ObjectId('{clientId}'),
                                        '{Reader.AUTHOR}': '{Reader.CLIENT}',
                                        '{Reader.IS_PERMITTED}': true,
                                        '{Reader.FIELDS}': {fields}
                                    }}
                                ]
                            ]
                        }}
                    }}
                }}");

            if (pipeline == null) pipeline = f(pipelineDefinition);
            else pipeline = f(pipeline);
        }
        if (tokenPrivileges.UpdatesFields.Length > 0)
        {
            string fields = "";
            tokenPrivileges.UpdatesFields.ToList().ConvertAll<string>(f => $"{{'{Field.NAME}': '{f.Name}', '{Field.IS_PERMITTED}': {f.IsPermitted.ToString().ToLower()}}}").ToList().ForEach(f => fields += f + ", ");
            fields = "[" + fields + "]";

            Func<PipelineDefinition<User, User>, PipelineDefinition<User, User>> f = (p) => p
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.UPDATERS}': {{
                            $filter: {{
                                'input': '${User.USER_PERMISSIONS}.{UserPermissions.UPDATERS}',
                                'as': 'item',
                                'cond': {{ 
                                    '$ne': [
                                        '$$item.{Updater.AUTHOR_ID}', ObjectId('{clientId}')
                                    ]
                                }}
                            }}
                        }}
                    }}
                }}")
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.UPDATERS}': {{ $concatArrays: [ '${User.USER_PERMISSIONS}.{UserPermissions.UPDATERS}',
                                [
                                    {{
                                        '{Updater.AUTHOR_ID}': ObjectId('{clientId}'),
                                        '{Updater.AUTHOR}': '{Updater.CLIENT}',
                                        '{Updater.IS_PERMITTED}': true,
                                        '{Updater.FIELDS}': {fields}
                                    }}
                                ]
                            ]
                        }}
                    }}
                }}");

            if (pipeline == null) pipeline = f(pipelineDefinition);
            else pipeline = f(pipeline);
        }
        if (tokenPrivileges.DeletesUser)
        {
            Func<PipelineDefinition<User, User>, PipelineDefinition<User, User>> f = (p) => p
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.DELETERS}': {{
                            $filter: {{
                                'input': '${User.USER_PERMISSIONS}.{UserPermissions.DELETERS}',
                                'as': 'item',
                                'cond': {{ 
                                    '$ne': [
                                        '$$item.{Deleter.AUTHOR_ID}', ObjectId('{clientId}')
                                    ]
                                }}
                            }}
                        }}
                    }}
                }}")
                .AppendStage<User, User, User>(@$"{{
                    '$addFields': {{
                        '{User.USER_PERMISSIONS}.{UserPermissions.DELETERS}': {{ $concatArrays: [ '${User.USER_PERMISSIONS}.{UserPermissions.DELETERS}',
                                [
                                    {{
                                        '{Deleter.AUTHOR_ID}': ObjectId('{clientId}'),
                                        '{Deleter.AUTHOR}': '{Deleter.CLIENT}',
                                        '{Deleter.IS_PERMITTED}': true
                                    }}
                                ]
                            ]
                        }}
                    }}
                }}");

            if (pipeline == null) pipeline = f(pipelineDefinition);
            else pipeline = f(pipeline);
        }

        Func<PipelineDefinition<User, User>, PipelineDefinition<User, User>> u = (p) => p
            .AppendStage<User, User, User>(@$"{{
                    $addFields: {{
                        {User.UPDATED_AT}: ISODate('{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")}')
                    }}
                }}");

        if (pipeline != null) pipeline = u(pipeline);

        return pipeline!;
    }

    public async Task<bool?> UpdateAuthorizingClient(string userId, AuthorizingClient authorizingClient)
    {
        UpdateResult r;
        try { r = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Set<AuthorizingClient>(User.AUTHORIZING_CLIENT, authorizingClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> AddAuthorizedClientWithTransaction(string userId, AuthorizedClient authorizedClient)
    {
        UpdateResult r = await _userCollection.UpdateOneAsync(_session, Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Push<AuthorizedClient>(User.AUTHORIZED_CLIENTS, authorizedClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow));

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> AddAuthorizedClient(string userId, AuthorizedClient authorizedClient)
    {
        UpdateResult r = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Push<AuthorizedClient>(User.AUTHORIZED_CLIENTS, authorizedClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow));

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateToken(string userId, string clientId, Token token)
    {
        FilterDefinition<User> filter = Builders<User>.Filter.And(new FilterDefinition<User>[] {
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
            Builders<User>.Filter.Eq(User.AUTHORIZED_CLIENTS + "."+AuthorizedClient.CLIENT_ID, ObjectId.Parse(clientId))
        });

        UpdateDefinition<User> update = Builders<User>.Update.Set<Token>(x => x.AuthorizedClients.FirstMatchingElement().Token, token).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow);

        UpdateResult r;
        try { r = await _userCollection.UpdateOneAsync(filter, update); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> UpdateUserPrivileges(string authorId, string userId, UserPermissions userPrivileges)
    {
        FilterDefinition<User> filters = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } }),
            GetUpdaterFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } })
        );

        UpdateResult r;
        try { r = await _userCollection.UpdateOneAsync(filters, Builders<User>.Update.Set(User.USER_PERMISSIONS, userPrivileges).Set(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> Update(string actorId, Data.Logics.Filter? filters, IEnumerable<Data.Logics.Update> updates, bool forClients = false)
    {
        List<FilterDefinition<User>>? filterDefinitions = new();

        List<Field> requiredFilterFields = new() { };
        if (filters is not null)
        {
            filterDefinitions.Add(Logics.Filter<User>.BuildDefinition(filters));
            requiredFilterFields = filters.GetFields().ToList().ConvertAll((f) => new Field() { Name = f, IsPermitted = true });
        }

        filterDefinitions.Add(GetReaderFilterDefinition(actorId, forClients, requiredFilterFields.Any() ? requiredFilterFields : null));

        UpdateDefinition<User>? updateDefinitions = Logics.Update<User>.BuildDefinition(updates).Set(User.UPDATED_AT, DateTime.UtcNow);
        List<Field> updateFieldsList = updates.ToList().ConvertAll((f) => new Field() { Name = f.Field, IsPermitted = true });
        foreach (Field field in User.GetProtectedFieldsAgainstMassUpdating())
            if (updateFieldsList.FirstOrDefault(f => f != null && f.Name == field.Name, null) != null)
                return false;

        filterDefinitions.Add(GetUpdaterFilterDefinition(actorId, forClients, updateFieldsList.Any() ? updateFieldsList : null));

        UpdateResult result;
        try { result = await _userCollection.UpdateManyAsync(Builders<User>.Filter.And(filterDefinitions.ToArray()), updateDefinitions); }
        catch (MongoDuplicateKeyException) { throw new DuplicationException(); }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { throw new DuplicationException(); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (result.IsAcknowledged && result.MatchedCount == 0) return null;

        return result.IsAcknowledged && result.MatchedCount > 0 && result.ModifiedCount > 0;
    }

    public async Task<bool?> Delete(string actorId, string id, bool forClients = false)
    {
        DeleteResult r;
        try { r = await _userCollection.DeleteOneAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)), GetDeleterFilterDefinition(actorId, forClients))); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.DeletedCount == 0) return null;

        return r.IsAcknowledged && r.DeletedCount == 1;
    }

    private IAggregateFluent<BsonDocument> RetrievePipeline(IAggregateFluent<BsonDocument> pipe, string actorId) =>
        pipe
            .AppendStage<BsonDocument>(@$"{{
                $addFields: {{
                    reader: {{
                        $arrayElemAt: [
                        '${PartialUser.USER_PERMISSIONS}.{UserPermissions.READERS}',
                        {{
                            $indexOfArray: [
                            '${PartialUser.USER_PERMISSIONS}.{UserPermissions.READERS}.{Reader.AUTHOR_ID}',
                            ObjectId('{actorId}'),
                            ],
                        }},
                        ],
                    }},
                }}
            }}")
            .AppendStage<BsonDocument>(@$"{{
                $addFields: {{
                    readableFields: {{
                        $map: {{
                        input: '$reader.{Reader.FIELDS}',
                        in: '$$this.name',
                        }},
                    }},
                }}
            }}")
            .AppendStage<BsonDocument>(@$"{{
                $group: {{
                    _id: null,
                    docs: {{
                        $push: {{
                        $arrayToObject: {{
                            $filter: {{
                            input: {{
                                $objectToArray: '$$ROOT',
                            }},
                            as: 'item',
                            cond: {{
                                $or: [
                                    {{
                                        $eq: ['$$item.k', '_id'],
                                    }},
                                    {{
                                        $in: [
                                        '$$item.k',
                                        '$readableFields',
                                        ],
                                    }},
                                ]
                            }},
                            }},
                        }},
                        }},
                    }},
                }}
            }}")
            .Unwind("docs")
            .ReplaceRoot<BsonDocument>("$docs");

    private FilterDefinition<PartialUser> GetReaderFilterDefinitionForReads(string actorId, bool isClient, List<Field>? requiredFields = null, List<Field>? optionalFields = null)
    {
        FilterDefinitionBuilder<PartialUser> builder = Builders<PartialUser>.Filter;
        List<FilterDefinition<PartialUser>> filters = new() {
            builder.Eq(Reader.AUTHOR, isClient ? Reader.CLIENT : Reader.USER),
            builder.Eq(Reader.AUTHOR_ID, ObjectId.Parse(actorId)),
            builder.Eq(Reader.IS_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            filters.Add(builder.All(Reader.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            filters.Add(builder.In(Reader.FIELDS, optionalFields));

        List<FilterDefinition<PartialUser>> allFilters = new() {
            builder.Eq(PartialUser.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.ARE_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            allFilters.Add(builder.All(PartialUser.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            allFilters.Add(builder.In(PartialUser.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.FIELDS, optionalFields));

        return builder.Or(
                    builder.And(
                        builder.SizeGt(PartialUser.USER_PERMISSIONS + "." + UserPermissions.READERS, 0),
                        builder.ElemMatch(PartialUser.USER_PERMISSIONS + "." + UserPermissions.READERS, builder.And(filters))
                    ),
                    builder.And(allFilters)
                );
    }

    private FilterDefinition<User> GetReaderFilterDefinition(string actorId, bool isClient, List<Field>? requiredFields = null, List<Field>? optionalFields = null)
    {
        FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
        List<FilterDefinition<User>> filters = new() {
            builder.Eq(Reader.AUTHOR, isClient ? Reader.CLIENT : Reader.USER),
            builder.Eq(Reader.AUTHOR_ID, ObjectId.Parse(actorId)),
            builder.Eq(Reader.IS_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            filters.Add(builder.All(Reader.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            filters.Add(builder.In(Reader.FIELDS, optionalFields));

        List<FilterDefinition<User>> allFilters = new() {
            builder.Eq(User.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.ARE_PERMITTED, true)
        };

        if (requiredFields != null && requiredFields.Count != 0)
            allFilters.Add(builder.All(User.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.FIELDS, requiredFields));

        if (optionalFields != null && optionalFields.Count != 0)
            allFilters.Add(builder.In(User.USER_PERMISSIONS + "." + UserPermissions.ALL_READERS + "." + AllReaders.FIELDS, optionalFields));

        return builder.Or(
                    builder.And(
                        builder.SizeGt(User.USER_PERMISSIONS + "." + UserPermissions.READERS, 0),
                        builder.ElemMatch(User.USER_PERMISSIONS + "." + UserPermissions.READERS, builder.And(filters))
                    ),
                    builder.And(allFilters)
                );
    }

    private FilterDefinition<User> GetUpdaterFilterDefinition(string actorId, bool isClient, List<Field>? fields = null) => Builders<User>.Filter.Or(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.SizeGt(User.USER_PERMISSIONS + "." + UserPermissions.UPDATERS, 0),
                        Builders<User>.Filter.ElemMatch(User.USER_PERMISSIONS + "." + UserPermissions.UPDATERS,
                        (fields == null || fields.Count == 0) ?
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, ObjectId.Parse(actorId)),
                            Builders<User>.Filter.Eq(Updater.IS_PERMITTED, true)
                        ) :
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Updater.AUTHOR, isClient ? Updater.CLIENT : Updater.USER),
                            Builders<User>.Filter.Eq(Updater.AUTHOR_ID, ObjectId.Parse(actorId)),
                            Builders<User>.Filter.Eq(Updater.IS_PERMITTED, true),
                            Builders<User>.Filter.All(Updater.FIELDS, fields)
                        ))
                    ),
                    (fields == null || fields.Count == 0) ?
                    Builders<User>.Filter.Eq(User.USER_PERMISSIONS + "." + UserPermissions.ALL_UPDATERS + "." + AllUpdaters.ARE_PERMITTED, true) :
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(User.USER_PERMISSIONS + "." + UserPermissions.ALL_UPDATERS + "." + AllUpdaters.ARE_PERMITTED, true),
                        Builders<User>.Filter.All(User.USER_PERMISSIONS + "." + UserPermissions.ALL_UPDATERS + "." + AllUpdaters.FIELDS, fields)
                    )
                );

    private FilterDefinition<User> GetDeleterFilterDefinition(string actorId, bool isClient) => Builders<User>.Filter.And(
                    Builders<User>.Filter.SizeGt(User.USER_PERMISSIONS + "." + UserPermissions.DELETERS, 0),
                    Builders<User>.Filter.ElemMatch(User.USER_PERMISSIONS + "." + UserPermissions.DELETERS,
                        Builders<User>.Filter.And(
                            Builders<User>.Filter.Eq(Deleter.AUTHOR, isClient ? Deleter.CLIENT : Deleter.USER),
                            Builders<User>.Filter.Eq(Deleter.AUTHOR_ID, ObjectId.Parse(actorId)),
                            Builders<User>.Filter.Eq(Deleter.IS_PERMITTED, true)
                        )
                    )
                );
}
