namespace user_management.Tests;

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using Moq;
using user_management.Controllers;
using user_management.Dtos.User;
using user_management.Models;
using Xunit;

[Collection("Controller")]
public class UserPrivilegesControllerTest
{
    public ControllerFixture ControllerFixture { get; private set; }

    public UserPrivilegesControllerTest(ControllerFixture controllerFixture) => ControllerFixture = controllerFixture;

    private UserPrivilegesController InstantiateUserController() => new UserPrivilegesController(ControllerFixture.IMapper.Object, ControllerFixture.IUserRepository.Object, ControllerFixture.IAuthHelper.Object);

    [Fact]
    public async Task api_user_privileges_update_readers()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Id = id.ToString() };
        User actorUser = new() { Id = actorId };
        User updatingUser = new() { Id = id, UserPrivileges = new() };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(actorId.ToString()));

        ControllerFixture.IMapper.Setup<UserPrivileges>(im => im.Map<UserPrivileges>(userPrivilegesDto)).Returns(userPrivileges);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(actorUser));
        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), false)).Returns(Task.FromResult<User?>(updatingUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), It.IsAny<UserPrivileges>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateReaders(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_all_readers()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Id = id.ToString() };
        User actorUser = new() { Id = actorId };
        User updatingUser = new() { Id = id, UserPrivileges = new() };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(actorId.ToString()));

        ControllerFixture.IMapper.Setup<UserPrivileges>(im => im.Map<UserPrivileges>(userPrivilegesDto)).Returns(userPrivileges);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(actorUser));
        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), false)).Returns(Task.FromResult<User?>(updatingUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), It.IsAny<UserPrivileges>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateAllReaders(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_updaters()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Id = id.ToString() };
        User actorUser = new() { Id = actorId };
        User updatingUser = new() { Id = id, UserPrivileges = new() };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(actorId.ToString()));

        ControllerFixture.IMapper.Setup<UserPrivileges>(im => im.Map<UserPrivileges>(userPrivilegesDto)).Returns(userPrivileges);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(actorUser));
        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), false)).Returns(Task.FromResult<User?>(updatingUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), It.IsAny<UserPrivileges>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateUpdaters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_all_updaters()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Id = id.ToString() };
        User actorUser = new() { Id = actorId };
        User updatingUser = new() { Id = id, UserPrivileges = new() };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(actorId.ToString()));

        ControllerFixture.IMapper.Setup<UserPrivileges>(im => im.Map<UserPrivileges>(userPrivilegesDto)).Returns(userPrivileges);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(actorUser));
        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), false)).Returns(Task.FromResult<User?>(updatingUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), It.IsAny<UserPrivileges>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateAllUpdaters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_deleters()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Id = id.ToString() };
        User actorUser = new() { Id = actorId };
        User updatingUser = new() { Id = id, UserPrivileges = new() };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(actorId.ToString()));

        ControllerFixture.IMapper.Setup<UserPrivileges>(im => im.Map<UserPrivileges>(userPrivilegesDto)).Returns(userPrivileges);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(actorUser));
        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), false)).Returns(Task.FromResult<User?>(updatingUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.Is<ObjectId>(o => o.ToString() == actorId.ToString()), It.Is<ObjectId>(o => o.ToString() == id.ToString()), It.IsAny<UserPrivileges>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateDeleters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }
}