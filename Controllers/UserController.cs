namespace user_management.Controllers;
using Microsoft.AspNetCore.Mvc;
using user_management.Data.User;
[ApiController]
[Route("api")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// fullName: <first name>-<middle name>-<last name>, for example: foo-bar-zar or foo--bar for no middle name
    /// </summary>
    [HttpGet("full-name-unique-check/{fullName}")]
    public async Task<IActionResult> FullNameUniqueCheck(string fullName) => (await _userRepository.RetrieveByFullNameForUniqueCheck(fullName)) == null ? NotFound() : Ok();

    [HttpGet("username-unique-check/{username}")]
    public async Task<IActionResult> UsernameUniqueCheck(string username) => (await _userRepository.RetrieveByUsernameForUniqueCheck(username)) == null ? NotFound() : Ok();

    [HttpGet("email-unique-check/{email}")]
    public async Task<IActionResult> EmailUniqueCheck(string email) => (await _userRepository.RetrieveByEmailForUniqueCheck(email)) == null ? NotFound() : Ok();

    [HttpGet("phone-number-unique-check/{phoneNumber}")]
    public async Task<IActionResult> PhoneNumberUniqueCheck(string phoneNumber) => (await _userRepository.RetrieveByPhoneNumberForUniqueCheck(phoneNumber)) == null ? NotFound() : Ok();

}
