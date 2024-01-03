using Newtonsoft.Json;
using user_management.Data.InMemory.Logics;
using user_management.Models;
using user_management.Services.Data.User;

namespace user_management.Data.InMemory.User;

public class UserRepository : InMemoryAtomicity, IUserRepository
{
    public UserRepository(InMemoryContext inMemoryContext) : base(inMemoryContext) { }

    public async Task<bool?> AddAuthorizedClient(string userId, AuthorizedClient authorizedClient)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == userId);
        if (user is null) return null;

        user.AuthorizedClients = user.AuthorizedClients.Append(authorizedClient).ToArray();
        user.UpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> AddAuthorizedClientWithTransaction(string userId, AuthorizedClient authorizedClient) => await AddAuthorizedClient(userId, authorizedClient);

    public async Task<bool?> AddTokenPrivilegesToUser(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o.Id == userId
            && o.UserPermissions.Readers.FirstOrDefault(r =>
                r.IsPermitted
                && r.AuthorId == authorId
                && r.Author == Reader.USER
                && r.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
            ) != null
            && o.UserPermissions.Updaters.FirstOrDefault(u =>
                u.IsPermitted
                && u.AuthorId == authorId
                && u.Author == Reader.USER
                && u.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
            ) != null
            );
        if (user is null) return null;

        if (tokenPrivileges.ReadsFields.Length > 0)
            user.UserPermissions.Readers = user.UserPermissions.Readers.Where(r => r.AuthorId != clientId).Append(new() { Author = Reader.CLIENT, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields }).ToArray();
        if (tokenPrivileges.UpdatesFields.Length > 0)
            user.UserPermissions.Updaters = user.UserPermissions.Updaters.Where(r => r.AuthorId != clientId).Append(new() { Author = Updater.CLIENT, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields }).ToArray();
        if (tokenPrivileges.DeletesUser)
            user.UserPermissions.Deleters = user.UserPermissions.Deleters.Where(r => r.AuthorId != clientId).Append(new() { Author = Deleter.CLIENT, AuthorId = clientId, IsPermitted = true }).ToArray();

        user.UpdatedAt = DateTime.UtcNow;

        await InMemoryContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool?> AddTokenPrivilegesToUserWithTransaction(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges) => await AddTokenPrivilegesToUser(userId, authorId, clientId, tokenPrivileges);

    public async Task<bool?> ChangeEmail(string email, string newEmail)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.Email = newEmail;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> ChangePassword(string email, string hashedPassword)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.Password = hashedPassword;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> ChangePhoneNumber(string email, string phoneNumber)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.PhoneNumber = phoneNumber;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> ChangeUsername(string email, string username)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.Username = username;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<Models.User?> Create(Models.User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await InMemoryContext.Users.AddAsync(user);
        await InMemoryContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool?> Delete(string actorId, string id, bool forClients = false)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o.Id == id
            &&
            o.UserPermissions.Deleters.FirstOrDefault(d =>
                d.IsPermitted
                && d.AuthorId == actorId
                && d.Author == (forClients ? Deleter.CLIENT : Deleter.USER)
            ) != null
        );
        if (user is null) return null;

        InMemoryContext.Remove(user);
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public string GenerateId() => InMemoryContext.Users.Any() ? (long.Parse(InMemoryContext.Users.LastOrDefault()!.Id) + 1).ToString() : "0";

    public int GetEstimatedDocumentCount() => InMemoryContext.Users.Count();

    public async Task<bool?> Login(string userId)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == userId);
        if (user is null) return null;

        user.LoggedOutAt = null;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> Logout(string id)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == id);
        if (user is null) return null;

        user.LoggedOutAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> RemoveAllClients(string userId, string authorId, bool isClient)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o.Id == userId
            && o.UserPermissions.Updaters.FirstOrDefault(u => u.IsPermitted && u.Author == (isClient ? Updater.CLIENT : Updater.USER) && u.AuthorId == authorId && u.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.AUTHORIZED_CLIENTS) != null) != null
            && o.UserPermissions.AllUpdaters != null
            && o.UserPermissions.AllUpdaters.ArePermitted
            && o.UserPermissions.AllUpdaters.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.AUTHORIZED_CLIENTS) != null
        );
        if (user is null) return null;

        user.AuthorizedClients = Array.Empty<AuthorizedClient>();
        user.UpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> RemoveClient(string userId, string clientId, string authorId, bool isClient)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o.Id == userId
            && o.UserPermissions.Updaters.FirstOrDefault(u => u.IsPermitted && u.Author == (isClient ? Updater.CLIENT : Updater.USER) && u.AuthorId == authorId && u.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.AUTHORIZED_CLIENTS) != null) != null
            && o.UserPermissions.AllUpdaters != null
            && o.UserPermissions.AllUpdaters.ArePermitted
            && o.UserPermissions.AllUpdaters.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.AUTHORIZED_CLIENTS) != null
        );
        if (user is null) return null;

        user.AuthorizedClients = user.AuthorizedClients.Where(a => a.ClientId != clientId).ToArray();
        user.UpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    private IEnumerable<Models.User> GetUsers(string actorId, Data.Logics.Filter? filters, bool forClients, IEnumerable<Models.User>? users = null)
    {
        users ??= InMemoryContext.Users.ToList();

        IEnumerable<Field> filterFields = filters is null ? Array.Empty<Field>() : filters.GetFields().ToList().ConvertAll(s => new Field() { IsPermitted = true, Name = s });
        bool filterFunc(dynamic o) => filters is null ?
            true :
            Filter<Models.User>.BuildDefinition(filters)(o)
        ;

        System.Console.WriteLine(string.Join(',', filterFields.ToList().ConvertAll(o => o.Name)));

        bool func(dynamic o) => IsAuthorizedToRead((o as Models.User)!, filterFields, actorId, forClients) && filterFunc(o);

        return users.Where(func).ToList();
    }

    public Task<List<PartialUser>> Retrieve(string actorId, Data.Logics.Filter? filters, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false)
    {
        if (limit <= 0) limit = 5;

        IEnumerable<Models.User> users = InMemoryContext.Users.ToList();

        if (sortBy is not null && ascending)
            users = users.OrderBy(o => sortBy).OrderBy(o => o.Id);
        else if (sortBy is not null && !ascending)
            users = users.OrderByDescending(o => sortBy).OrderByDescending(o => o.Id);
        else if (sortBy is null)
            users = users.OrderBy(o => o.Id, new IdComparer());

        users = GetUsers(actorId, filters, forClients, users);

        users = users.Skip(iteration * limit).Take(limit);

        // In-memory db is not designed with performance in mind.
        string usersJson = JsonConvert.SerializeObject(users);
        List<PartialUser>? partialUsers = JsonConvert.DeserializeObject<List<PartialUser>>(usersJson);

        return Task.FromResult(partialUsers!);
    }

    public Task<Models.User?> RetrieveByClientIdAndCode(string clientId, string code) =>
        Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o != null
            && o.AuthorizingClient != null
            && o.AuthorizingClient.ClientId == clientId
            && o.AuthorizingClient.Code == code
        , null));

    public Task<Models.User?> RetrieveByEmailForExistenceCheck(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public Task<Models.User?> RetrieveByFullNameForExistenceCheck(string? firstName, string? middleName, string? lastName) =>
        Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o =>
            o != null
            && o.FirstName == firstName
            && o.MiddleName == middleName
            && o.LastName == lastName
        , null));

    public Task<Models.User?> RetrieveById(string id) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Id == id, null));

    public Task<PartialUser?> RetrieveById(string actorId, string id, bool forClients = false)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(u =>
            u.Id == id
            && (
                u.UserPermissions.Readers.FirstOrDefault(r => r.IsPermitted && r.AuthorId == actorId && r.Author == (forClients ? Reader.CLIENT : Reader.USER) && r.Fields.Any()) != null
                || (
                    u.UserPermissions.AllReaders != null
                    && u.UserPermissions.AllReaders.ArePermitted
                    && u.UserPermissions.AllReaders.Fields.Any()
                )
            )
        );

        if (user is null)
            return Task.FromResult<PartialUser?>(null);

        string userJson = JsonConvert.SerializeObject(user);
        PartialUser? partialUsers = JsonConvert.DeserializeObject<PartialUser>(userJson);

        return Task.FromResult<PartialUser?>(partialUsers!);
    }

    public Task<Models.User?> RetrieveByIdForAuthenticationHandling(string userId) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Id == userId, null));

    public Task<Models.User?> RetrieveByIdForAuthorizationHandling(string id) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Id == id, null));

    public Task<Models.User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.PhoneNumber == phoneNumber, null));

    public Task<Models.User?> RetrieveByRefreshTokenValue(string value) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.AuthorizedClients.FirstOrDefault(a => a.RefreshToken.Value == value) != null, null));

    public Task<Models.User?> RetrieveByTokenValue(string value) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.AuthorizedClients.FirstOrDefault(a => a.Token.Value == value) != null, null));

    public Task<Models.User?> RetrieveByUsernameForExistenceCheck(string username) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Username == username, null));

    public Task<Models.User?> RetrieveUserByLoginCredentials(string? email, string? username) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && (username != null ? o.Username == username : o.Email == email), null));

    public Task<Models.User?> RetrieveUserForEmailChange(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public Task<Models.User?> RetrieveUserForPasswordChange(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public Task<Models.User?> RetrieveUserForPhoneNumberChange(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public Task<Models.User?> RetrieveUserForUnverifiedEmailChange(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public Task<Models.User?> RetrieveUserForUsernameChange(string email) => Task.FromResult(InMemoryContext.Users.ToList().FirstOrDefault(o => o != null && o.Email == email, null));

    public async Task<bool?> Update(string actorId, Data.Logics.Filter? filters, IEnumerable<Data.Logics.Update> updates, bool forClients = false)
    {
        IEnumerable<Models.User> users = GetUsers(actorId, filters, forClients);

        Func<object, object> updateFunc = Update<Models.User>.BuildDefinition(updates);

        users.ToList().ForEach(user => user = (updateFunc(user) as Models.User)!);

        await InMemoryContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool?> UpdateAuthorizingClient(string userId, AuthorizingClient authorizingClient)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == userId);
        if (user is null) return null;

        user.AuthorizingClient = authorizingClient;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> UpdateToken(string userId, string clientId, Token token)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == userId && o.AuthorizedClients.FirstOrDefault(a => a.ClientId == clientId) != null);
        if (user is null) return null;

        user.AuthorizedClients.First(a => a.ClientId == clientId).Token = token;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> UpdateUserPrivileges(string authorId, string userId, UserPermissions userPrivileges)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == userId
            && o.UserPermissions.Readers.FirstOrDefault(
                u => u.IsPermitted
                && u.Author == Reader.USER
                && u.AuthorId == authorId
                && u.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
                ) != null
            && o.UserPermissions.AllReaders != null
            && o.UserPermissions.AllReaders.ArePermitted
            && o.UserPermissions.AllReaders.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
            && o.UserPermissions.Updaters.FirstOrDefault(
                u => u.IsPermitted
                && u.Author == Updater.USER
                && u.AuthorId == authorId
                && u.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
                ) != null
            && o.UserPermissions.AllUpdaters != null
            && o.UserPermissions.AllUpdaters.ArePermitted
            && o.UserPermissions.AllUpdaters.Fields.FirstOrDefault(f => f.IsPermitted && f.Name == Models.User.USER_PERMISSIONS) != null
        );
        if (user is null) return null;

        user.UserPermissions = userPrivileges;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> UpdateVerificationSecret(string verificationSecret, string email)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.VerificationSecret = verificationSecret;
        user.VerificationSecretUpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> UpdateVerificationSecretForActivation(string verificationSecret, string email)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email && o.IsEmailVerified == true);
        if (user is null) return null;

        user.VerificationSecret = verificationSecret;
        user.VerificationSecretUpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> UpdateVerificationSecretForPasswordChange(string verificationSecret, string email)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Email == email);
        if (user is null) return null;

        user.VerificationSecret = verificationSecret;
        user.VerificationSecretUpdatedAt = DateTime.UtcNow;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool?> Verify(string id)
    {
        Models.User? user = InMemoryContext.Users.ToList().FirstOrDefault(o => o.Id == id);
        if (user is null) return null;

        user.IsEmailVerified = true;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    private static bool IsAuthorizedToRead(Models.User o, IEnumerable<Field> filterFields, string actorId, bool forClients) => o != null
            && o != null
            && (
                o.UserPermissions.Readers.FirstOrDefault(r =>
                    r.IsPermitted
                    && r.AuthorId == actorId
                    && r.Author == (forClients ? Reader.CLIENT : Reader.USER)
                    && (
                        !filterFields.Any()
                        || (
                            filterFields.Any()
                            && !filterFields.Where(f => r.Fields.FirstOrDefault(rf => rf.Name == f.Name) is null).Any()
                        )
                    )
                ) != null
                || (
                    o.UserPermissions.AllReaders != null
                    && o.UserPermissions.AllReaders!.ArePermitted
                    && filterFields.FirstOrDefault(f => !o.UserPermissions.AllReaders!.Fields.Where(rf => rf.Name == f.Name).Any()) == null
                )
            );
}
