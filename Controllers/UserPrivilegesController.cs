namespace user_management.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Authorization.Attributes;
using user_management.Data.User;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserPrivilegesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthHelper _authHelper;
    private readonly IMapper _mapper;

    public UserPrivilegesController(IMapper mapper, IUserRepository userRepository, IAuthHelper authHelper)
    {
        _userRepository = userRepository;
        _authHelper = authHelper;
        _mapper = mapper;
    }

    private async Task<User?> GetAuthenticatedUser()
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return null;

        string? actorId = await _authHelper.GetIdentifier(User, _userRepository);

        if (actorId == null || !ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return null;

        return await _userRepository.RetrieveById(actorObjectId, actorObjectId, _authHelper.GetAuthenticationType(User) != "JWT");
    }

    [Permissions(Permissions = new string[] { "update_readers" })]
    [HttpPatch("update-readers")]
    public async Task<IActionResult> UpdateReaders(UserPrivilegesPatchDto userPrivilegesDto)
    {
        UserPrivileges userPrivileges = _mapper.Map<UserPrivileges>(userPrivilegesDto);
        if (!ObjectId.TryParse(userPrivilegesDto.Id, out ObjectId userId)) return BadRequest();

        User? user = await GetAuthenticatedUser();
        if (user == null) return Unauthorized();

        User? updatingUser = await _userRepository.RetrieveById((ObjectId)user.Id!, userId, _authHelper.GetAuthenticationType(User) != "JWT");
        if (updatingUser == null) return NotFound();

        updatingUser.UserPrivileges!.Readers = userPrivileges.Readers;

        bool? result = await _userRepository.UpdateUserPrivileges((ObjectId)user.Id!, (ObjectId)updatingUser.Id!, updatingUser.UserPrivileges, _authHelper.GetAuthenticationType(User) != "JWT");
        if (result == null) return NotFound();
        if (result == false) return Problem();

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_all_readers" })]
    [HttpPatch("update-all-readers")]
    public async Task<IActionResult> UpdateAllReaders(UserPrivilegesPatchDto userPrivilegesDto)
    {
        UserPrivileges userPrivileges = _mapper.Map<UserPrivileges>(userPrivilegesDto);
        if (!ObjectId.TryParse(userPrivilegesDto.Id, out ObjectId userId)) return BadRequest();

        User? user = await GetAuthenticatedUser();
        if (user == null) return Unauthorized();

        User? updatingUser = await _userRepository.RetrieveById((ObjectId)user.Id!, userId, _authHelper.GetAuthenticationType(User) != "JWT");
        if (updatingUser == null) return NotFound();

        updatingUser.UserPrivileges!.AllReaders = userPrivileges.AllReaders;

        bool? result = await _userRepository.UpdateUserPrivileges((ObjectId)user.Id!, (ObjectId)updatingUser.Id!, updatingUser.UserPrivileges, _authHelper.GetAuthenticationType(User) != "JWT");
        if (result == null) return NotFound();
        if (result == false) return Problem();

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_updaters" })]
    [HttpPatch("update-updaters")]
    public async Task<IActionResult> UpdateUpdaters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        UserPrivileges userPrivileges = _mapper.Map<UserPrivileges>(userPrivilegesDto);
        if (!ObjectId.TryParse(userPrivilegesDto.Id, out ObjectId userId)) return BadRequest();

        User? user = await GetAuthenticatedUser();
        if (user == null) return Unauthorized();

        User? updatingUser = await _userRepository.RetrieveById((ObjectId)user.Id!, userId, _authHelper.GetAuthenticationType(User) != "JWT");
        if (updatingUser == null) return NotFound();

        updatingUser.UserPrivileges!.Updaters = userPrivileges.Updaters;

        bool? result = await _userRepository.UpdateUserPrivileges((ObjectId)user.Id!, (ObjectId)updatingUser.Id!, updatingUser.UserPrivileges, _authHelper.GetAuthenticationType(User) != "JWT");
        if (result == null) return NotFound();
        if (result == false) return Problem();

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_all_updaters" })]
    [HttpPatch("update-all-updaters")]
    public async Task<IActionResult> UpdateAllUpdaters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        UserPrivileges userPrivileges = _mapper.Map<UserPrivileges>(userPrivilegesDto);
        if (!ObjectId.TryParse(userPrivilegesDto.Id, out ObjectId userId)) return BadRequest();

        User? user = await GetAuthenticatedUser();
        if (user == null) return Unauthorized();

        User? updatingUser = await _userRepository.RetrieveById((ObjectId)user.Id!, userId, _authHelper.GetAuthenticationType(User) != "JWT");
        if (updatingUser == null) return NotFound();

        updatingUser.UserPrivileges!.AllUpdaters = userPrivileges.AllUpdaters;

        bool? result = await _userRepository.UpdateUserPrivileges((ObjectId)user.Id!, (ObjectId)updatingUser.Id!, updatingUser.UserPrivileges, _authHelper.GetAuthenticationType(User) != "JWT");
        if (result == null) return NotFound();
        if (result == false) return Problem();

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_deleters" })]
    [HttpPatch("update-deleters")]
    public async Task<IActionResult> UpdateDeleters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        UserPrivileges userPrivileges = _mapper.Map<UserPrivileges>(userPrivilegesDto);
        if (!ObjectId.TryParse(userPrivilegesDto.Id, out ObjectId userId)) return BadRequest();

        User? user = await GetAuthenticatedUser();
        if (user == null) return Unauthorized();

        User? updatingUser = await _userRepository.RetrieveById((ObjectId)user.Id!, userId, _authHelper.GetAuthenticationType(User) != "JWT");
        if (updatingUser == null) return NotFound();

        updatingUser.UserPrivileges!.Deleters = userPrivileges.Deleters;

        bool? result = await _userRepository.UpdateUserPrivileges((ObjectId)user.Id!, (ObjectId)updatingUser.Id!, updatingUser.UserPrivileges, _authHelper.GetAuthenticationType(User) != "JWT");
        if (result == null) return NotFound();
        if (result == false) return Problem();

        return Ok();
    }
}