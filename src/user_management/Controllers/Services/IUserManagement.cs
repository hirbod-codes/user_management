using user_management.Dtos.User;

namespace user_management.Controllers.Services;

public interface IUserManagement
{
    /// <exception cref="ArgumentException"></exception>
    public Task<bool> FullNameExistenceCheck(string? firstName, string? middleName, string? lastName);
    public Task<bool> UsernameExistenceCheck(string username);
    public Task<bool> EmailExistenceCheck(string email);
    public Task<bool> PhoneNumberExistenceCheck(string phoneNumber);

    /// <exception cref="System.Net.Mail.SmtpException"></exception>
    /// <exception cref="user_management.Services.SmtpFailureException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<Models.User> Register(UserCreateDto userDto);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    /// <exception cref="System.Net.Mail.SmtpException"></exception>
    /// <exception cref="user_management.Services.SmtpFailureException"></exception>
    public Task SendVerificationEmail(string email);

    /// <exception cref="System.Net.Mail.SmtpException"></exception>
    /// <exception cref="user_management.Services.SmtpFailureException"></exception>
    public Task Notify(string email, string message);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.VerificationCodeExpiredException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidVerificationCodeException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidPasswordException"></exception>
    public Task Activate(Activation activatingUser);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidPasswordException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task ChangeUnverifiedEmail(ChangeUnverifiedEmail dto);

    /// <exception cref="user_management.Services.Data.User.PasswordConfirmationMismatchException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.VerificationCodeExpiredException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidVerificationCodeException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task ChangePassword(ChangePassword dto);

    /// <exception cref="user_management.Services.Data.User.MissingCredentialException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidPasswordException"></exception>
    /// <exception cref="user_management.Services.Data.User.UnverifiedUserException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task<LoginResult> Login(Login loggingInUser);

    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task Logout(string identifier);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.VerificationCodeExpiredException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidVerificationCodeException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task ChangeUsername(ChangeUsername dto);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.VerificationCodeExpiredException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidVerificationCodeException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task ChangeEmail(ChangeEmail dto);

    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.User.VerificationCodeExpiredException"></exception>
    /// <exception cref="user_management.Services.Data.User.InvalidVerificationCodeException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task ChangePhoneNumber(ChangePhoneNumber dto);

    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public Task RemoveClient(string clientId, string userId, string authorId, bool forClients);


    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public Task RemoveClients(string userId, string authorId, bool forClients);

    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    public Task<Models.PartialUser> RetrieveById(string actorId, string userId, bool forClients);

    /// <exception cref="ArgumentException"></exception>
    public Task<List<Models.PartialUser>> Retrieve(string actorId, bool forClients, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true);

    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task Update(string actorId, UserPatchDto userPatchDto, bool forClients);

    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task Delete(string actorId, string id, bool forClients);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="System.UnauthorizedAccessException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    public Task<IEnumerable<UserClientRetrieveDto>> RetrieveClientsById(string authorId, string userId, bool forClients);
}
