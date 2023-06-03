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
    private readonly IMapper _mapper;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthHelper _authHelper;
    private readonly IStringHelper _stringHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenController(IMapper mapper, IClientRepository clientRepository, IUserRepository userRepository, IStringHelper stringHelper, IAuthHelper authHelper, IDateTimeProvider dateTimeProvider)
    {
        _mapper = mapper;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _stringHelper = stringHelper;
        _authHelper = authHelper;
        _dateTimeProvider = dateTimeProvider;
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
}