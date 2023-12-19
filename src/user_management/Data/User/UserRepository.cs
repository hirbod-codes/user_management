namespace user_management.Data.User;

using MongoDB.Driver;
using user_management.Models;
using user_management.Services.Data.User;
using MongoDB.Bson;
using user_management.Data.Logics.Filter;
using user_management.Data.Logics.Update;
using user_management.Services.Data;
using MongoDB.Driver.Linq;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<PartialUser> _partialUserCollection;

    public UserRepository(MongoCollections mongoCollections)
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

    public async Task<List<PartialUser>> Retrieve(string actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false)
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

        SortDefinition<PartialUser> sort;
        if (ascending)
            sort = Builders<PartialUser>.Sort.Ascending(sortBy == null ? PartialUser.UPDATED_AT : sortBy);
        else
            sort = Builders<PartialUser>.Sort.Descending(sortBy == null ? PartialUser.UPDATED_AT : sortBy);

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

    public async Task<bool?> AddTokenPrivilegesToUser(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null)
    {
        FilterDefinition<User> filterDefinition = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
            GetReaderFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } }),
            GetUpdaterFilterDefinition(authorId, false, new() { new() { IsPermitted = true, Name = User.USER_PERMISSIONS } })
        );

        if (tokenPrivileges.ReadsFields.Length == 0 && tokenPrivileges.UpdatesFields.Length == 0 && !tokenPrivileges.DeletesUser) return null;

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

        UpdateDefinition<User> updateDefinition = Builders<User>.Update.Pipeline(pipeline);

        UpdateResult r;
        try
        {
            if (session != null) r = await _userCollection.UpdateOneAsync(session, filterDefinition, updateDefinition);
            else r = await _userCollection.UpdateOneAsync(filterDefinition, updateDefinition);
        }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> UpdateAuthorizingClient(string userId, AuthorizingClient authorizingClient)
    {
        UpdateResult r;
        try { r = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Set<AuthorizingClient>(User.AUTHORIZING_CLIENT, authorizingClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.MatchedCount == 1 && r.ModifiedCount == 1;
    }

    public async Task<bool?> AddAuthorizedClient(string userId, AuthorizedClient authorizedClient, IClientSessionHandle? session = null)
    {
        UpdateResult r;
        try
        {
            r = await (session == null
                ? _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Push<AuthorizedClient>(User.AUTHORIZED_CLIENTS, authorizedClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow))
                : _userCollection.UpdateOneAsync(session, Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), Builders<User>.Update.Push<AuthorizedClient>(User.AUTHORIZED_CLIENTS, authorizedClient).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow))
            );
        }
        catch (Exception) { throw new DatabaseServerException(); }

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
        try { r = await _userCollection.UpdateOneAsync(filters, Builders<User>.Update.Set<UserPermissions>(User.USER_PERMISSIONS, userPrivileges).Set<User, DateTime>(User.UPDATED_AT, DateTime.UtcNow)); }
        catch (Exception) { throw new DatabaseServerException(); }

        if (r.IsAcknowledged && r.MatchedCount == 0) return null;

        return r.IsAcknowledged && r.ModifiedCount == 1 && r.MatchedCount == 1;
    }

    public async Task<bool?> Update(string actorId, string filtersString, string updatesString, bool forClients = false)
    {
        List<FilterDefinition<User>>? filters = new List<FilterDefinition<User>>();

        FilterDefinition<User> filter = Builders<User>.Filter.Empty;
        List<string> requiredFilterFieldsList = new List<string> { };
        List<string> optionalFilterFieldsList = new List<string> { };
        List<Field> requiredFilterFields = new List<Field> { };
        List<Field> optionalFilterFields = new List<Field> { };
        if (filtersString != "empty")
        {
            IFilterLogic<User> iLogic = FilterLogics<User>.BuildILogic(filtersString);
            filters.Add(iLogic.BuildDefinition());
            requiredFilterFieldsList = iLogic.GetRequiredFields();
            optionalFilterFieldsList = iLogic.GetOptionalFields();
            requiredFilterFields = requiredFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
            optionalFilterFields = optionalFilterFieldsList.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        }

        filters.Add(GetReaderFilterDefinition(actorId, forClients, requiredFilterFields, optionalFilterFields));

        UpdateLogics<User> logic = new UpdateLogics<User>();
        UpdateDefinition<User>? updates = logic.BuildILogic(updatesString).BuildDefinition().Set(User.UPDATED_AT, DateTime.UtcNow);
        List<Field> updateFieldsList = logic.Fields.ConvertAll<Field>((f) => new Field() { Name = f, IsPermitted = true });
        foreach (Field field in User.GetProtectedFieldsAgainstMassUpdating())
            if (updateFieldsList.FirstOrDefault<Field?>(f => f != null && f.Name == field.Name, null) != null)
                return false;

        filters.Add(GetUpdaterFilterDefinition(actorId, forClients, updateFieldsList));

        UpdateResult result;
        try { result = await _userCollection.UpdateManyAsync(Builders<User>.Filter.And(filters.ToArray()), updates); }
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
