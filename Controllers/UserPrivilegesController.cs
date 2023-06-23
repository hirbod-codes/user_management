namespace user_management.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Authorization.Attributes;
using user_management.Data;
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

    [Permissions(Permissions = new string[] { StaticData.UPDATE_READERS })]
    [HttpPatch("update-readers")]
    public async Task<IActionResult> UpdateReaders(UserPrivilegesPatchDto userPrivilegesDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT")
            return StatusCode(403);

        if (userPrivilegesDto.Readers == null)
            return BadRequest();

        string? authorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (authorId == null)
            return Unauthorized();
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))
            return Unauthorized();

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null)
            return NotFound();
        if (author.UserPrivileges == null)
            return Problem();

        author.UserPrivileges.Readers = _mapper.Map<Reader[]>(userPrivilegesDto.Readers);

        bool? r = await _userRepository.UpdateUserPrivileges(author);

        if (r == null || r == false)
            return Problem();
        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_READERS })]
    [HttpPatch("update-all-readers")]
    public async Task<IActionResult> UpdateAllReaders(UserPrivilegesPatchDto userPrivilegesDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT")
            return BadRequest();

        if (userPrivilegesDto.AllReaders == null)
            return BadRequest();

        string? authorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (authorId == null)
            return Unauthorized();
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))
            return Unauthorized();

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null)
            return NotFound();
        if (author.UserPrivileges == null)
            return Problem();

        author.UserPrivileges.AllReaders = _mapper.Map<AllReaders>(userPrivilegesDto.AllReaders);

        bool? r = await _userRepository.UpdateUserPrivileges(author);

        if (r == null || r == false)
            return Problem();
        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_UPDATERS })]
    [HttpPatch("update-updaters")]
    public async Task<IActionResult> UpdateUpdaters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT")
            return BadRequest();

        if (userPrivilegesDto.Updaters == null)
            return BadRequest();

        string? authorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (authorId == null)
            return Unauthorized();
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))
            return Unauthorized();

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null)
            return NotFound();
        if (author.UserPrivileges == null)
            return Problem();

        author.UserPrivileges.Updaters = _mapper.Map<Updater[]>(userPrivilegesDto.Updaters);

        bool? r = await _userRepository.UpdateUserPrivileges(author);

        if (r == null || r == false)
            return Problem();
        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_ALL_UPDATERS })]
    [HttpPatch("update-all-updaters")]
    public async Task<IActionResult> UpdateAllUpdaters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT")
            return BadRequest();

        if (userPrivilegesDto.AllUpdaters == null)
            return BadRequest();

        string? authorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (authorId == null)
            return Unauthorized();
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))
            return Unauthorized();

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null)
            return NotFound();
        if (author.UserPrivileges == null)
            return Problem();

        author.UserPrivileges.AllUpdaters = _mapper.Map<AllUpdaters>(userPrivilegesDto.AllUpdaters);

        bool? r = await _userRepository.UpdateUserPrivileges(author);

        if (r == null || r == false)
            return Problem();
        return Ok();
    }

    [Permissions(Permissions = new string[] { StaticData.UPDATE_DELETERS })]
    [HttpPatch("update-deleters")]
    public async Task<IActionResult> UpdateDeleters(UserPrivilegesPatchDto userPrivilegesDto)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT")
            return BadRequest();

        if (userPrivilegesDto.Deleters == null)
            return BadRequest();

        string? authorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (authorId == null)
            return Unauthorized();
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))
            return Unauthorized();

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null)
            return NotFound();
        if (author.UserPrivileges == null)
            return Problem();

        author.UserPrivileges.Deleters = _mapper.Map<Deleter[]>(userPrivilegesDto.Deleters);

        bool? r = await _userRepository.UpdateUserPrivileges(author);

        if (r == null || r == false)
            return Problem();
        return Ok();
    }
}