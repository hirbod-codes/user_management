namespace user_management.Controllers;

using System.Security.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.Client;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;

[ApiController]
[ApiVersion("1.0")]
[Route("api")]
public class ClientController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IClientManagement _clientManagement;
    private readonly IAuthenticatedByJwt _authenticatedByJwt;

    public ClientController(IMapper mapper, IClientManagement clientManagement, IAuthenticatedByJwt authenticatedByJwt)
    {
        _mapper = mapper;
        _clientManagement = clientManagement;
        _authenticatedByJwt = authenticatedByJwt;
    }

    /// <summary>
    /// Register a new client.
    /// </summary>
    /// <param name="clientCreateDto">The registering client's information.</param>
    [Permissions(Permissions = new string[] { StaticData.REGISTER_CLIENT })]
    [HttpPost(PATH_POST_REGISTER)]
    [SwaggerResponse(statusCode: 200, type: typeof(ClientRetrieveDto))]
    public async Task<IActionResult> Register([FromBody] ClientCreateDto clientCreateDto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        (Client client, string? notHashedSecret) result;

        try { result = await _clientManagement.Register(_mapper.Map<Client>(clientCreateDto)); }
        catch (DuplicationException) { return Problem("System failed to register your client."); }
        catch (DatabaseServerException) { return Problem("System failed to register your client."); }
        catch (RegistrationFailure) { return Problem("System failed to register your client."); }

        ClientRetrieveDto clientRetrieveDto = _mapper.Map<ClientRetrieveDto>(result.client);
        clientRetrieveDto.Secret = result.notHashedSecret;

        return Ok(clientRetrieveDto);
    }

    /// <summary>
    /// Retrieve client's public information.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.READ_CLIENT })]
    [HttpGet(PATH_GET_PUBLIC_INFO)]
    [SwaggerResponse(statusCode: 200, type: typeof(ClientPublicInfoRetrieveDto))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> RetrieveClientPublicInfo([FromRoute] string id)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveClientPublicInfo(id); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientPublicInfoRetrieveDto>(client));
    }

    /// <summary>
    /// Retrieve a client's information.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.READ_CLIENT })]
    [HttpGet(PATH_GET_RETRIEVE)]
    [SwaggerResponse(statusCode: 200, type: typeof(ClientRetrieveDto))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Retrieve([FromRoute] string secret)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveBySecret(secret); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientRetrieveDto>(client));
    }

    /// <summary>
    /// Update a client.
    /// </summary>
    /// <remarks>To Update client's redirect url, server needs client's secret.</remarks>
    [HttpPatch(PATH_PATCH)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    public async Task<IActionResult> Update([FromBody] ClientPatchDto dto)
    {
        try { await _clientManagement.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl); }
        catch (ArgumentException ex) { return ex.ParamName == "clientId" ? BadRequest("Invalid id for client provided.") : Problem("Internal server error encountered."); }
        catch (DuplicationException) { return BadRequest("The provided redirect url is not unique!"); }
        catch (DatabaseServerException) { return Problem("We failed to update this client."); }

        return Ok();
    }

    /// <summary>
    /// Delete a client.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.DELETE_CLIENT })]
    [HttpDelete(PATH_DELETE)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Delete([FromRoute] string secret)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { await _clientManagement.DeleteBySecret(secret); }
        catch (ArgumentException) { return BadRequest("Invalid secret provided."); }
        catch (DataNotFoundException) { return NotFound(); }

        return Ok();
    }

    /// <summary>
    /// Token exposure. 
    /// </summary>
    /// <remarks>If a token or secret is exposed to unauthorized parties, this endpoint must be called.
    /// Following actions will be taken:<br/>
    /// 1. The client's current secret will be replaced with a new one.<br/>
    /// 2. The client's current ID will be replaced with a new one, therefor all the users that has previously authorized this client, must authorized it again.
    /// 
    /// Note: if a client is exposed more than 2 times, it will be banned from asking users' authorization and token generation.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_CLIENT })]
    [HttpPatch(PATH_PATCH_EXPOSURE)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    public async Task<IActionResult> UpdateExposedClient([FromBody] ClientExposedDto dto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { return Ok(await _clientManagement.UpdateExposedClient(dto.ClientId, dto.Secret)); }
        catch (ArgumentException) { return BadRequest(); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (DataNotFoundException) { return StatusCode(403); }
        catch (OperationException) { return Problem("Internal server error encountered."); }
        catch (DuplicationException) { return Problem("Internal server error encountered."); }
        catch (DatabaseServerException) { return Problem("Internal server error encountered."); }
    }

    public const string PATH_POST_REGISTER = "client";
    public const string PATH_GET_PUBLIC_INFO = "client/info/{id}";
    public const string PATH_GET_RETRIEVE = "client/{secret}";
    public const string PATH_PATCH = "client";
    public const string PATH_PATCH_EXPOSURE = "client/exposure";
    public const string PATH_DELETE = "client/{secret}";
}
