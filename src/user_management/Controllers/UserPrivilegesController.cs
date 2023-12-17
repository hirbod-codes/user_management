namespace user_management.Controllers;

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class UserPrivilegesController : ControllerBase
{
    private readonly IUserPrivilegesManagement _userPrivilegesManagement;
    private readonly IAuthenticated _authenticated;

    public UserPrivilegesController(IUserPrivilegesManagement userPrivilegesManagement, IAuthenticated authenticated)
    {
        _userPrivilegesManagement = userPrivilegesManagement;
        _authenticated = authenticated;
    }

    /// <summary>
    /// Update user's privileges' Readers field.
    /// </summary>
    /// <remarks>
    /// Only UserId and Readers fields are used in this endpoint.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS })]
    [HttpPatch(UPDATE_READERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> UpdateReaders([FromBody] UserPrivilegesPatchDto dto)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateReaders(_authenticated.GetAuthenticatedIdentifier(), dto); }
        catch (ArgumentException ex) { return ex.ParamName == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Update user's privileges' AllReaders field.
    /// </summary>
    /// <remarks>
    /// Only UserId and AllReaders fields are used in this endpoint.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_READERS })]
    [HttpPatch(UPDATE_ALL_READERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> UpdateAllReaders([FromBody] UserPrivilegesPatchDto dto)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateAllReaders(_authenticated.GetAuthenticatedIdentifier(), dto); }
        catch (ArgumentException ex) { return ex.ParamName == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Update user's privileges' Updaters field.
    /// </summary>
    /// <remarks>
    /// Only UserId and Updaters fields are used in this endpoint.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_UPDATERS })]
    [HttpPatch(UPDATE_UPDATERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> UpdateUpdaters([FromBody] UserPrivilegesPatchDto dto)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateUpdaters(_authenticated.GetAuthenticatedIdentifier(), dto); }
        catch (ArgumentException ex) { return ex.ParamName == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Update user's privileges' AllUpdaters field.
    /// </summary>
    /// <remarks>
    /// Only UserId and AllUpdaters fields are used in this endpoint.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_UPDATERS })]
    [HttpPatch(UPDATE_ALL_UPDATERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> UpdateAllUpdaters([FromBody] UserPrivilegesPatchDto dto)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateAllUpdaters(_authenticated.GetAuthenticatedIdentifier(), dto); }
        catch (ArgumentException ex) { return ex.ParamName == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Update user's privileges' Deleters field.
    /// </summary>
    /// <remarks>
    /// Only UserId and Deleters fields are used in this endpoint.
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_DELETERS })]
    [HttpPatch(UPDATE_DELETERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> UpdateDeleters([FromBody] UserPrivilegesPatchDto dto)
    {
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userPrivilegesManagement.UpdateDeleters(_authenticated.GetAuthenticatedIdentifier(), dto); }
        catch (ArgumentException ex) { return ex.ParamName == "authorId" ? Unauthorized() : BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Retrieve a list of fields that are assignable to Readers and AllReaders fields.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(READER_ASSIGNABLE_FIELDS)]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Field>))]
    public IActionResult ReaderAssignableFields() => Ok(Models.User.GetReadableFields());

    /// <summary>
    /// Retrieve a list of fields that are assignable to Updaters and AllUpdaters fields.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS, StaticData.UPDATE_ALL_READERS })]
    [HttpGet(UPDATER_ASSIGNABLE_FIELDS)]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Field>))]
    public IActionResult UpdaterAssignableFields() => Ok(Models.User.GetUpdatableFields());

    public const string UPDATE_READERS = "update-readers";
    public const string UPDATE_ALL_READERS = "update-all-readers";
    public const string UPDATE_UPDATERS = "update-updaters";
    public const string UPDATE_ALL_UPDATERS = "update-all-updaters";
    public const string UPDATE_DELETERS = "update-deleters";
    public const string READER_ASSIGNABLE_FIELDS = "reader-assignable-fields";
    public const string UPDATER_ASSIGNABLE_FIELDS = "updater-assignable-fields";
}
