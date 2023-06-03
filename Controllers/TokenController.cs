namespace user_management.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Authorization.Attributes;
using user_management.Data.Client;
using user_management.Data.User;
using user_management.Dtos.Token;
using user_management.Models;
using user_management.Utilities;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class TokenController : ControllerBase
{
    public const int REFRESH_TOKEN_EXPIRATION_MONTHS = 2;
    public const int CODE_EXPIRATION_MINUTES = 3;
    public const int TOKEN_EXPIRATION = 1;
    private readonly IMapper _mapper;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMongoClient _mongoClient;
    private readonly IAuthHelper _authHelper;
    private readonly IStringHelper _stringHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenController(IMapper mapper, IClientRepository clientRepository, IUserRepository userRepository, IStringHelper stringHelper, IAuthHelper authHelper, IDateTimeProvider dateTimeProvider, IMongoClient iMongoClient)
    {
        _mapper = mapper;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _stringHelper = stringHelper;
        _authHelper = authHelper;
        _dateTimeProvider = dateTimeProvider;
        _mongoClient = iMongoClient;
    }

    [Permissions(Permissions = new string[] { "authorize_client" })]
    [HttpPost("auth")]
    public async Task<ActionResult> Authorize(TokenAuthDto tokenAuthDto)
    {
        if (tokenAuthDto.State.Length < 40 || tokenAuthDto.ResponseType != "code" || (new List<string>() { "SHA256", "SHA512" }).FirstOrDefault<string?>(s => s != null && s == tokenAuthDto.CodeChallengeMethod) == null) return BadRequest();

        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return NotFound();
        if (!ObjectId.TryParse(id, out ObjectId userId)) return BadRequest();

        if (!ObjectId.TryParse(tokenAuthDto.ClientId, out ObjectId clientObjectId)) return BadRequest();

        User? user = await _userRepository.RetrieveById(userId, userId); if (user == null) return NotFound();

        TokenPrivileges scope = _mapper.Map<TokenPrivileges>(tokenAuthDto.Scope);

        if ((await _clientRepository.RetrieveByIdAndRedirectUrl(clientObjectId, tokenAuthDto.RedirectUrl)) == null) return NotFound();

        string code = null!;
        bool again = false;
        int safety = 0;
        do
        {
            try
            {
                code = _stringHelper.GenerateRandomString(128);
                await _userRepository.AddClientById(
                    user,
                    clientObjectId,
                    scope,
                    _dateTimeProvider.ProvideUtcNow().AddMonths(REFRESH_TOKEN_EXPIRATION_MONTHS),
                    _stringHelper.GenerateRandomString(128),
                    _dateTimeProvider.ProvideUtcNow().AddMinutes(CODE_EXPIRATION_MINUTES),
                    code,
                    tokenAuthDto.CodeChallenge,
                    tokenAuthDto.CodeChallengeMethod
                );
                again = false;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { again = true; }
            safety++;
        } while (again && safety < 200);

        if (safety >= 200) return Problem();

        return RedirectPermanent(tokenAuthDto.RedirectUrl + $"?code={code}&state={tokenAuthDto.State}");
    }

    [HttpPost("token")]
    public async Task<ActionResult<object>> Token(TokenCreateDto tokenCreateDto)
    {
        if (tokenCreateDto.GrantType != "authorization_code") return BadRequest("only 'authorization_code' grant type is supported");

        Client? client = await _clientRepository.RetrieveByIdAndRedirectUrl(ObjectId.Parse(tokenCreateDto.ClientId), tokenCreateDto.RedirectUrl);
        if (client == null) return NotFound();

        User? user = await _userRepository.RetrieveByClientIdAndCode((ObjectId)client.Id!, tokenCreateDto.Code);
        if (user == null) return NotFound();

        UserClient? userClient = user.Clients.ToList().FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == client.Id, null);
        if (userClient == null) return NotFound();

        RefreshToken? refreshToken = userClient.RefreshToken;
        if (refreshToken == null) return NotFound();
        if (refreshToken.ExpirationDate! < _dateTimeProvider.ProvideUtcNow() || (refreshToken.CodeExpiresAt != null && refreshToken.CodeExpiresAt < _dateTimeProvider.ProvideUtcNow())) return BadRequest();
        if (_stringHelper.HashWithoutSalt(tokenCreateDto.CodeVerifier, refreshToken.CodeChallengeMethod!) != _stringHelper.Base64Decode(refreshToken.CodeChallenge!)) return BadRequest();

        string tokenValue = null!;
        using (IClientSessionHandle session = await _mongoClient.StartSessionAsync())
        {
            TransactionOptions transactionOptions = new(writeConcern: WriteConcern.WMajority);

            session.StartTransaction(transactionOptions);

            bool? userResult = await _userRepository.AddTokenPrivileges(user, (ObjectId)client.Id, refreshToken.TokenPrivileges!, session);
            if (userResult == null) return NotFound();
            if (userResult == false) return Problem();

            bool again = false;
            int safety = 0;
            do
            {
                try
                {
                    tokenValue = _stringHelper.GenerateRandomString(128);

                    bool? addTokenResult = await _userRepository.AddToken(user, (ObjectId)client.Id!, _stringHelper.HashWithoutSalt(tokenValue)!, _dateTimeProvider.ProvideUtcNow().AddHours(TOKEN_EXPIRATION), session);
                    if (addTokenResult == null)
                    {
                        await session.AbortTransactionAsync(); return NotFound();
                    }
                    if (addTokenResult == false)
                    {
                        await session.AbortTransactionAsync(); return Problem();
                    }
                    again = false;
                }
                catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { again = true; }
                safety++;
            } while (again && safety < 200);

            if (safety >= 200)
            {
                await session.AbortTransactionAsync();
                return Problem();
            }

            await session.CommitTransactionAsync();
        }

        return Ok(new { access_token = tokenValue, refresh_token = refreshToken.Value });
    }
}