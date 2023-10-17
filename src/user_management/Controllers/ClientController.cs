namespace user_management.Controllers;

using System.Security.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.Client;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;
using user_management.Validation.Attributes;

[ApiController]
[Route("api")]
[Produces("application/json")]
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

    [Permissions(Permissions = new string[] { StaticData.REGISTER_CLIENT })]
    [HttpPost(PATH_POST_REGISTER)]
    public async Task<ActionResult> Register(ClientCreateDto clientCreateDto)
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

    [Permissions(Permissions = new string[] { StaticData.READ_CLIENT })]
    [HttpGet(PATH_GET_PUBLIC_INFO)]
    public async Task<ActionResult> RetrieveClientPublicInfo(string id)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveClientPublicInfo(id); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientPublicInfoRetrieveDto>(client));
    }

    [Permissions(Permissions = new string[] { StaticData.READ_CLIENT })]
    [HttpGet(PATH_GET_RETRIEVE)]
    public async Task<ActionResult> Retrieve(string secret)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveBySecret(secret); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientRetrieveDto>(client));
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_CLIENT })]
    [HttpPatch(PATH_PATCH)]
    public async Task<ActionResult> Update([FromBody] ClientPatchDto dto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { await _clientManagement.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException ex) { return ex.Message == "clientId" ? BadRequest("Invalid id for client provided.") : Problem("Internal server error encountered."); }
        catch (DuplicationException) { return BadRequest("The provided redirect url is not unique!"); }
        catch (DatabaseServerException) { return Problem("We failed to update this client."); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.DELETE_CLIENT })]
    [HttpDelete(PATH_DELETE)]
    public async Task<ActionResult> Delete(string secret)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { await _clientManagement.DeleteBySecret(secret); }
        catch (ArgumentException) { return BadRequest("Invalid secret provided."); }
        catch (DataNotFoundException) { return NotFound(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_CLIENT })]
    [HttpPatch(PATH_PATCH_EXPOSURE)]
    public async Task<IActionResult> UpdateExposedClient(ClientExposedDto dto)
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
