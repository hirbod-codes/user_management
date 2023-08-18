namespace user_management.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Services;
using user_management.Services.Data;
using user_management.Services.Data.User;
using user_management.Utilities;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserPrivilegesController : ControllerBase
{
    private readonly IAuthHelper _authHelper;
    private readonly IUserPrivilegesManagement _userPrivilegesManagement;

    public UserPrivilegesController(IAuthHelper authHelper, IUserPrivilegesManagement userPrivilegesManagement)
    {
        _authHelper = authHelper;
        _userPrivilegesManagement = userPrivilegesManagement;
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS })]
    [HttpPatch(UPDATE_READERS)]
    public async Task<IActionResult> UpdateReaders(UserPrivilegesPatchDto dto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? authorId = await _authHelper.GetIdentifier(User);
        if (authorId == null || !ObjectId.TryParse(authorId, out ObjectId authorObjectId)) return Unauthorized();

        try { await _userPrivilegesManagement.UpdateReaders(authorId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_READERS })]
    [HttpPatch(UPDATE_ALL_READERS)]
    public async Task<IActionResult> UpdateAllReaders(UserPrivilegesPatchDto dto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? authorId = await _authHelper.GetIdentifier(User);
        if (authorId == null || !ObjectId.TryParse(authorId, out ObjectId authorObjectId)) return Unauthorized();

        try { await _userPrivilegesManagement.UpdateAllReaders(authorId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_UPDATERS })]
    [HttpPatch(UPDATE_UPDATERS)]
    public async Task<IActionResult> UpdateUpdaters(UserPrivilegesPatchDto dto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? authorId = await _authHelper.GetIdentifier(User);
        if (authorId == null || !ObjectId.TryParse(authorId, out ObjectId authorObjectId)) return Unauthorized();

        try { await _userPrivilegesManagement.UpdateUpdaters(authorId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_UPDATERS })]
    [HttpPatch(UPDATE_ALL_UPDATERS)]
    public async Task<IActionResult> UpdateAllUpdaters(UserPrivilegesPatchDto dto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? authorId = await _authHelper.GetIdentifier(User);
        if (authorId == null || !ObjectId.TryParse(authorId, out ObjectId authorObjectId)) return Unauthorized();

        try { await _userPrivilegesManagement.UpdateAllUpdaters(authorId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_DELETERS })]
    [HttpPatch(UPDATE_DELETERS)]
    public async Task<IActionResult> UpdateDeleters(UserPrivilegesPatchDto dto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? authorId = await _authHelper.GetIdentifier(User);
        if (authorId == null || !ObjectId.TryParse(authorId, out ObjectId authorObjectId)) return Unauthorized();

        try { await _userPrivilegesManagement.UpdateDeleters(authorId, dto); }
        catch (ArgumentException ex) { return ex.Message == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(READER_ASSIGNABLE_FIELDS)]
    public IActionResult ReaderAssignableFields() => Ok(Models.User.ReaderAssignableFields());

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(UPDATER_ASSIGNABLE_FIELDS)]
    public IActionResult UpdaterAssignableFields() => Ok(Models.User.UpdaterAssignableFields());

    public const string UPDATE_READERS = "update-readers";
    public const string UPDATE_ALL_READERS = "update-all-readers";
    public const string UPDATE_UPDATERS = "update-updaters";
    public const string UPDATE_ALL_UPDATERS = "update-all-updaters";
    public const string UPDATE_DELETERS = "update-deleters";
    public const string READER_ASSIGNABLE_FIELDS = "reader-assignable-fields";
    public const string UPDATER_ASSIGNABLE_FIELDS = "updater-assignable-fields";
}