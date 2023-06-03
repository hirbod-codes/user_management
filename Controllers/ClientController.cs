namespace user_management.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using user_management.Authorization.Attributes;
using user_management.Data.Client;
using user_management.Dtos.Client;
using user_management.Models;
using user_management.Utilities;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IClientRepository _clientRepository;
    private readonly IStringHelper _stringHelper;
    private readonly IAuthHelper _authHelper;

    public ClientController(IMapper mapper, IClientRepository clientRepository, IStringHelper stringHelper, IAuthHelper authHelper)
    {
        _mapper = mapper;
        _clientRepository = clientRepository;
        _stringHelper = stringHelper;
        _authHelper = authHelper;
    }

    [Permissions(Permissions = new string[] { "register_client" })]
    [HttpPost]
    public async Task<ActionResult> Register(ClientCreateDto clientCreateDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        Client client = _mapper.Map<Client>(clientCreateDto);

        string secret = null!;
        bool again = false;
        int safety = 0;
        do
        {
            try
            {
                secret = _stringHelper.GenerateRandomString(60);

                client.Secret = _stringHelper.HashWithoutSalt(secret);
                if (secret == null) return Problem();

                client = await _clientRepository.Create(client);
                if (client == null) return Problem("System failed to register your account.");

                again = false;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey) { again = true; }
            safety++;
        } while (again && safety < 200);

        if (safety >= 200) return Problem();

        ClientRetrieveDto clientRetrieveDto = _mapper.Map<ClientRetrieveDto>(client);
        clientRetrieveDto.Secret = secret;

        return Ok(clientRetrieveDto);
    }

    [Permissions(Permissions = new string[] { "read_client" })]
    [HttpGet("client/{secret}")]
    public async Task<ActionResult> Retrieve(string secret)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
        if (hashedSecret == null) return BadRequest();

        Client? client = await _clientRepository.RetrieveBySecret(hashedSecret);
        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientRetrieveDto>(client));
    }
}