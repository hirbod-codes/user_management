using AutoMapper;
using user_management.Services.Data;
using user_management.Dtos.User;
using user_management.Utilities;
using System.Net.Mail;
using MongoDB.Bson;
using user_management.Services.Data.User;
using user_management.Authentication.JWT;
using user_management.Controllers.Services;

namespace user_management.Services;

public class UserManagement : IUserManagement
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IJWTAuthenticationHandler _jwtAuthenticationHandler;

    public UserManagement(IDateTimeProvider dateTimeProvider, INotificationHelper notificationHelper, IStringHelper stringHelper, IUserRepository userRepository, IMapper mapper, IJWTAuthenticationHandler jwtAuthenticationHandler)
    {
        _dateTimeProvider = dateTimeProvider;
        _notificationHelper = notificationHelper;
        _stringHelper = stringHelper;
        _userRepository = userRepository;
        _mapper = mapper;
        _jwtAuthenticationHandler = jwtAuthenticationHandler;
    }

    public async Task<bool> FullNameExistenceCheck(string firstName, string middleName, string lastName) => (await _userRepository.RetrieveByFullNameForExistenceCheck(firstName, middleName, lastName)) != null;
    public async Task<bool> UsernameExistenceCheck(string username) => (await _userRepository.RetrieveByUsernameForExistenceCheck(username)) != null;
    public async Task<bool> EmailExistenceCheck(string email) => (await _userRepository.RetrieveByEmailForExistenceCheck(email)) != null;
    public async Task<bool> PhoneNumberExistenceCheck(string phoneNumber) => (await _userRepository.RetrieveByPhoneNumberForExistenceCheck(phoneNumber)) != null;

    public async Task<Models.User> Register(UserCreateDto userDto)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        await Notify(userDto.Email, verificationMessage);

        Models.User? unverifiedUser = _mapper.Map<Models.User>(userDto);
        unverifiedUser.Password = _stringHelper.Hash(userDto.Password);
        unverifiedUser.VerificationSecret = verificationMessage;
        unverifiedUser.VerificationSecretUpdatedAt = _dateTimeProvider.ProvideUtcNow();
        unverifiedUser.IsVerified = false;

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
        Models.User? user = await _userRepository.RetrieveUserByLoginCredentials(activatingUser.Email, null);
        if (user == null) throw new DataNotFoundException();

        if ((bool)user.IsVerified!) return;

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (activatingUser.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        if (!_stringHelper.DoesHashMatch(user.Password!, activatingUser.Password)) throw new InvalidPasswordException();

        bool? r = await _userRepository.Verify((ObjectId)user.Id!);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangePassword(ChangePassword dto)
    {
        if (dto.Password != dto.PasswordConfirmation) throw new PasswordConfirmationMismatchException();

        Models.User? user = await _userRepository.RetrieveUserForPasswordChange(dto.Email);
        if (user == null) throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool? r = await _userRepository.ChangePassword(dto.Email, _stringHelper.Hash(dto.Password));
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task<(string jwt, object user)> Login(Login loggingInUser)
    {
        if (loggingInUser.Username == null && loggingInUser.Email == null) throw new MissingCredentialException();

        Models.User? user = await _userRepository.RetrieveUserByLoginCredentials(loggingInUser.Email, loggingInUser.Username);
        if (user == null) throw new DataNotFoundException();
        if (!_stringHelper.DoesHashMatch(user.Password!, loggingInUser.Password)) throw new InvalidPasswordException();

        if (user.IsVerified == false) throw new UnverifiedUserException();

        bool? r = await _userRepository.Login(user);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();

        string jwt = _jwtAuthenticationHandler.GenerateAuthenticationJWT(user.Id!.ToString()!);

        return (jwt, user.GetReadable((ObjectId)user.Id, _mapper));
    }

    public async Task Logout(string identifier)
    {
        if (!ObjectId.TryParse(identifier, out ObjectId id)) throw new ArgumentException();

        bool? r = await _userRepository.Logout(id);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangeUsername(ChangeUsername dto)
    {
        Models.User? user = await _userRepository.RetrieveUserForUsernameChange(dto.Email);
        if (user == null) throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool? r = await _userRepository.ChangeUsername(dto.Email, dto.Username);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangeEmail(ChangeEmail dto)
    {
        Models.User? user = await _userRepository.RetrieveUserForEmailChange(dto.Email);
        if (user == null) throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool? r = await _userRepository.ChangeEmail(dto.Email, dto.NewEmail);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task ChangePhoneNumber(ChangePhoneNumber dto)
    {
        Models.User? user = await _userRepository.RetrieveUserForPhoneNumberChange(dto.Email);
        if (user == null) throw new DataNotFoundException();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) throw new VerificationCodeExpiredException();

        if (dto.VerificationSecret != user.VerificationSecret) throw new InvalidVerificationCodeException();

        bool? r = await _userRepository.ChangePhoneNumber(dto.Email, dto.PhoneNumber);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task RemoveClient(string clientId, string userId)
    {
        if (!ObjectId.TryParse(userId, out ObjectId userObjectId)) throw new ArgumentException("userId");
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException("clientId");

        Models.User? user = await _userRepository.RetrieveById(userObjectId, userObjectId);
        if (user == null) throw new DataNotFoundException();

        bool? r = await _userRepository.RemoveClient(user, clientObjectId, userObjectId, false);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task RemoveClients(string userId)
    {
        if (!ObjectId.TryParse(userId, out ObjectId userObjectId)) throw new ArgumentException();

        Models.User? user = await _userRepository.RetrieveById(userObjectId, userObjectId);
        if (user == null) throw new DataNotFoundException();

        bool? r = await _userRepository.RemoveAllClients(user, userObjectId, false);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task<Models.User> RetrieveById(string actorId, string userId, bool forClients)
    {
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) throw new ArgumentException("actorId");
        if (!ObjectId.TryParse(userId, out ObjectId objectId)) throw new ArgumentException("userId");

        Models.User? user = await _userRepository.RetrieveById(actorObjectId, objectId, forClients);
        if (user == null) throw new DataNotFoundException();

        return user;
    }

    public async Task<List<Models.User>> Retrieve(string actorId, bool forClients, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) throw new ArgumentException();

        return await _userRepository.Retrieve(actorObjectId, logicsString, limit, iteration, sortBy, ascending, forClients);
    }

    public async Task Update(string actorId, UserPatchDto userPatchDto, bool forClients)
    {
        if (userPatchDto.UpdatesString == null || userPatchDto.FiltersString == null || userPatchDto.FiltersString == "empty") throw new ArgumentException("userPatchDto");

        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) throw new ArgumentException("actorId");

        bool? r = await _userRepository.Update(actorObjectId, userPatchDto.FiltersString, userPatchDto.UpdatesString, forClients);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task Delete(string actorId, string userId, bool forClients)
    {
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) throw new ArgumentException("actorId");
        if (!ObjectId.TryParse(userId, out ObjectId objectId)) throw new ArgumentException("userId");

        bool? r = await _userRepository.Delete(actorObjectId, objectId, forClients);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }
}