namespace user_management.Controllers;

using Microsoft.AspNetCore.Mvc;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Services;
using user_management.Services.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserPrivilegesController : ControllerBase
{
    private readonly IUserPrivilegesManagement _userPrivilegesManagement;
    private readonly IAuthenticated _authenticated;

    public UserPrivilegesController(IUserPrivilegesManagement userPrivilegesManagement, IAuthenticated authenticated)
    {
        _userPrivilegesManagement = userPrivilegesManagement;
        _authenticated = authenticated;
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS })]
    [HttpPatch(UPDATE_READERS)]
    public async Task<IActionResult> UpdateReaders(UserPrivilegesPatchDto dto, string userId)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateReaders(_authenticated.GetAuthenticatedIdentifier(), userId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_READERS })]
    [HttpPatch(UPDATE_ALL_READERS)]
    public async Task<IActionResult> UpdateAllReaders(UserPrivilegesPatchDto dto, string userId)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateAllReaders(_authenticated.GetAuthenticatedIdentifier(), userId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_UPDATERS })]
    [HttpPatch(UPDATE_UPDATERS)]
    public async Task<IActionResult> UpdateUpdaters(UserPrivilegesPatchDto dto, string userId)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateUpdaters(_authenticated.GetAuthenticatedIdentifier(), userId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_UPDATERS })]
    [HttpPatch(UPDATE_ALL_UPDATERS)]
    public async Task<IActionResult> UpdateAllUpdaters(UserPrivilegesPatchDto dto, string userId)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateAllUpdaters(_authenticated.GetAuthenticatedIdentifier(), userId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_DELETERS })]
    [HttpPatch(UPDATE_DELETERS)]
    public async Task<IActionResult> UpdateDeleters(UserPrivilegesPatchDto dto, string userId)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateDeleters(_authenticated.GetAuthenticatedIdentifier(), userId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(READER_ASSIGNABLE_FIELDS)]
    public IActionResult ReaderAssignableFields() => Ok(Models.User.GetReadableFields());

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(UPDATER_ASSIGNABLE_FIELDS)]
    public IActionResult UpdaterAssignableFields() => Ok(Models.User.GetUpdatableFields());

    public const string UPDATE_READERS = "update-readers";
    public const string UPDATE_ALL_READERS = "update-all-readers";
    public const string UPDATE_UPDATERS = "update-updaters";
    public const string UPDATE_ALL_UPDATERS = "update-all-updaters";
    public const string UPDATE_DELETERS = "update-deleters";
    public const string READER_ASSIGNABLE_FIELDS = "reader-assignable-fields";
    public const string UPDATER_ASSIGNABLE_FIELDS = "updater-assignable-fields";
}