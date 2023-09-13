namespace user_management.Controllers;

using System.Security.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Dtos.Client;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;

[ApiController]
[Route("api/[controller]")]
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

    [Permissions(Permissions = new string[] { "register_client" })]
    [HttpPost]
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

    [Permissions(Permissions = new string[] { "read_client" })]
    [HttpGet("info/{id}")]
    public async Task<ActionResult> RetrieveClientPublicInfo(string id)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveClientPublicInfo(id); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientRetrieveDto>(client));
    }

    [Permissions(Permissions = new string[] { "read_client" })]
    [HttpGet("{secret}")]
    public async Task<ActionResult> Retrieve(string secret)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        Client? client = null;

        try { client = await _clientManagement.RetrieveBySecret(secret); }
        catch (ArgumentException) { return BadRequest(); }

        if (client == null) return NotFound();

        return Ok(_mapper.Map<ClientRetrieveDto>(client));
    }

    [Permissions(Permissions = new string[] { "update_client" })]
    [HttpPatch]
    public async Task<ActionResult> Update(ClientPutDto clientPutDto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { await _clientManagement.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException ex) { return ex.Message == "clientId" ? BadRequest("Invalid id for client provided.") : Problem("Internal server error encountered."); }
        catch (DuplicationException) { return BadRequest("The provided redirect url is not unique!"); }
        catch (DatabaseServerException) { return Problem("We failed to update this client."); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { "delete_client" })]
    [HttpDelete]
    public async Task<ActionResult> Delete(ClientDeleteDto clientDeleteDto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        try { await _clientManagement.DeleteBySecret(clientDeleteDto.Id, clientDeleteDto.Secret); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException ex) { return ex.Message == "clientId" ? BadRequest("Invalid id for client provided.") : Problem("Internal server error encountered."); }
        catch (DataNotFoundException) { return NotFound(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_client" })]
    [HttpPatch("exposure")]
    public async Task<IActionResult> UpdateExposedClient(ClientExposedDto dto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        string newSecret = null!;
        try { newSecret = await _clientManagement.UpdateExposedClient(dto.ClientId, dto.Secret); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (DataNotFoundException) { return StatusCode(403); }
        catch (OperationException) { return Problem("Internal server error encountered."); }
        catch (DuplicationException) { return Problem("Internal server error encountered."); }
        catch (DatabaseServerException) { return Problem("Internal server error encountered."); }

        return Ok(newSecret);
    }
}