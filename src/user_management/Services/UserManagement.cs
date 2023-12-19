using AutoMapper;
using user_management.Services.Data;
using user_management.Dtos.User;
using user_management.Utilities;
using System.Net.Mail;
using user_management.Services.Data.User;
using user_management.Controllers.Services;
using user_management.Models;
using user_management.Services.Client;

namespace user_management.Services;

public class UserManagement : IUserManagement
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IAuthHelper _authHelper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IClientRepository _clientRepository;

    public UserManagement(IDateTimeProvider dateTimeProvider, INotificationHelper notificationHelper, IStringHelper stringHelper, IUserRepository userRepository, IMapper mapper, IAuthHelper authHelper, IClientRepository clientRepository)
    {
        _authHelper = authHelper;
        _dateTimeProvider = dateTimeProvider;
        _notificationHelper = notificationHelper;
        _stringHelper = stringHelper;
        _userRepository = userRepository;
        _clientRepository = clientRepository;
        _mapper = mapper;
    }

    public async Task<bool> FullNameExistenceCheck(string? firstName, string? middleName, string? lastName)
    {
        if (firstName == null && middleName == null && lastName == null) throw new ArgumentException("At least one of the parameter must not be null.");

        return (await _userRepository.RetrieveByFullNameForExistenceCheck(firstName, middleName, lastName)) != null;
    }

    public async Task<bool> UsernameExistenceCheck(string username) => (await _userRepository.RetrieveByUsernameForExistenceCheck(username)) != null;
    public async Task<bool> EmailExistenceCheck(string email) => (await _userRepository.RetrieveByEmailForExistenceCheck(email)) != null;
    public async Task<bool> PhoneNumberExistenceCheck(string phoneNumber) => (await _userRepository.RetrieveByPhoneNumberForExistenceCheck(phoneNumber)) != null;

    public async Task<User> Register(UserCreateDto userDto)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        await Notify(userDto.Email, verificationMessage);

        User? unverifiedUser = _mapper.Map<User>(userDto);
        unverifiedUser.Id = _userRepository.GenerateId();
        unverifiedUser.UserPermissions = await UserPermissions.GetDefault(unverifiedUser.Id, _clientRepository);
        unverifiedUser.Password = _stringHelper.Hash(userDto.Password);
        unverifiedUser.VerificationSecret = verificationMessage;
        unverifiedUser.VerificationSecretUpdatedAt = _dateTimeProvider.ProvideUtcNow();
        unverifiedUser.IsEmailVerified = false;

        unverifiedUser = await _userRepository.Create(unverifiedUser);

        if (unverifiedUser == null) throw new OperationException();

        return unverifiedUser;
    }

    public async Task SendVerificationEmail(string email)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        await _userRepository.UpdateVerificationSecret(verificationMessage, email);

        await Notify(email, verificationMessage);
    }

    public async Task Notify(string email, string message)
    {
        try { await _notificationHelper.SendVerificationMessage(email, message); }
        catch (SmtpException) { throw; }
        catch (System.Exception) { throw new SmtpFailureException(); }
    }

    public async Task Activate(Activation activatingUser)
    {
        User? user = await _userRepository.RetrieveUserByLoginCredentials(activatingUser.Email, null);
        if (user == null) throw new DataNotFoundException();

        if ((bool)user.IsEmailVerified!) return;

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (activatingUser.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        if (!_stringHelper.DoesHashMatch(user.Password, activatingUser.Password)) throw new InvalidPasswordException();

        bool r = await _userRepository.Verify(user.Id) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangePassword(ChangePassword dto)
    {
        if (dto.Password != dto.PasswordConfirmation) throw new PasswordConfirmationMismatchException();

        User user = await _userRepository.RetrieveUserForPasswordChange(dto.Email) ?? throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool r = await _userRepository.ChangePassword(dto.Email, _stringHelper.Hash(dto.Password)) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task<LoginResult> Login(Login loggingInUser)
    {
        if (loggingInUser.Username == null && loggingInUser.Email == null) throw new MissingCredentialException();

        User user = await _userRepository.RetrieveUserByLoginCredentials(loggingInUser.Email, loggingInUser.Username) ?? throw new DataNotFoundException();

        if (!_stringHelper.DoesHashMatch(user.Password, loggingInUser.Password)) throw new InvalidPasswordException();

        if (user.IsEmailVerified == false) throw new UnverifiedUserException();

        bool r = await _userRepository.Login(user.Id) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();

        string jwt = _authHelper.GenerateAuthenticationJWT(user.Id.ToString()!);

        return new() { Jwt = jwt, UserId = user.Id.ToString() };
    }

    public async Task Logout(string id)
    {
        bool r = await _userRepository.Logout(id) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangeUnverifiedEmail(ChangeUnverifiedEmail dto)
    {
        User user = await _userRepository.RetrieveUserForUnverifiedEmailChange(dto.Email) ?? throw new DataNotFoundException();

        if (!_stringHelper.DoesHashMatch(user.Password, dto.Password)) throw new InvalidPasswordException();

        bool r = await _userRepository.ChangeEmail(dto.Email, dto.NewEmail) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangeUsername(ChangeUsername dto)
    {
        User user = await _userRepository.RetrieveUserForUsernameChange(dto.Email) ?? throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool r = await _userRepository.ChangeUsername(dto.Email, dto.Username) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangeEmail(ChangeEmail dto)
    {
        User user = await _userRepository.RetrieveUserForEmailChange(dto.Email) ?? throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool r = await _userRepository.ChangeEmail(dto.Email, dto.NewEmail) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangePhoneNumber(ChangePhoneNumber dto)
    {
        User user = await _userRepository.RetrieveUserForPhoneNumberChange(dto.Email) ?? throw new DataNotFoundException();
        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool r = await _userRepository.ChangePhoneNumber(dto.Email, dto.PhoneNumber) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task RemoveClient(string clientId, string userId, string authorId, bool forClients)
    {
        bool r = await _userRepository.RemoveClient(userId, clientId, authorId, forClients) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task RemoveClients(string userId, string authorId, bool forClients)
    {
        bool r = await _userRepository.RemoveAllClients(userId, authorId, forClients) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task<PartialUser> RetrieveById(string actorId, string userId, bool forClients)
    {
        PartialUser user = await _userRepository.RetrieveById(actorId, userId, forClients) ?? throw new DataNotFoundException();
        return user;
    }

    public async Task<IEnumerable<AuthorizedClientRetrieveDto>> RetrieveClientsById(string actorId, string userId, bool forClients)
    {
        PartialUser user = await _userRepository.RetrieveById(actorId, userId, forClients) ?? throw new DataNotFoundException();

        if (!user.IsAuthorizedClientsTouched()) throw new UnauthorizedAccessException();

        return user.AuthorizedClients == null ? Array.Empty<AuthorizedClientRetrieveDto>() : user.AuthorizedClients.ToList().ConvertAll<AuthorizedClientRetrieveDto>(c => _mapper.Map<AuthorizedClientRetrieveDto>(c));
    }

    public async Task<List<PartialUser>> Retrieve(string actorId, bool forClients, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        return await _userRepository.Retrieve(actorId, logicsString, limit, iteration, sortBy, ascending, forClients);
    }

    public async Task Update(string actorId, UserPatchDto userPatchDto, bool forClients)
    {
        if (userPatchDto.UpdatesString == null || userPatchDto.FiltersString == null || userPatchDto.FiltersString == "empty") throw new ArgumentException(null, nameof(userPatchDto));

        bool r = await _userRepository.Update(actorId, userPatchDto.FiltersString, userPatchDto.UpdatesString, forClients) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task Delete(string actorId, string userId, bool forClients)
    {
        bool r = await _userRepository.Delete(actorId, userId, forClients) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }
}
