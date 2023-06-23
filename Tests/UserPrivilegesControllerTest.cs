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
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Readers = new ReaderPatchDto[] { } };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(id.ToString()));

        ControllerFixture.IMapper.Setup<Reader[]>(im => im.Map<Reader[]>(userPrivilegesDto.Readers)).Returns(new Reader[] { });

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>())).Returns(Task.FromResult<User?>(new User() { UserPrivileges = new UserPrivileges() { } }));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.IsAny<User>())).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateReaders(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_all_readers()
    {
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { AllReaders = new AllReaders() { } };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(id.ToString()));

        ControllerFixture.IMapper.Setup<AllReaders>(im => im.Map<AllReaders>(userPrivilegesDto.AllReaders)).Returns(new AllReaders() { });

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>())).Returns(Task.FromResult<User?>(new User() { UserPrivileges = new UserPrivileges() { } }));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.IsAny<User>())).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateAllReaders(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_updaters()
    {
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Updaters = new UpdaterPatchDto[] { } };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(id.ToString()));

        ControllerFixture.IMapper.Setup<Updater[]>(im => im.Map<Updater[]>(userPrivilegesDto.Updaters)).Returns(new Updater[] { });

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>())).Returns(Task.FromResult<User?>(new User() { UserPrivileges = new UserPrivileges() { } }));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.IsAny<User>())).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateUpdaters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_all_updaters()
    {
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { AllUpdaters = new AllUpdaters() { } };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(id.ToString()));

        ControllerFixture.IMapper.Setup<AllUpdaters>(im => im.Map<AllUpdaters>(userPrivilegesDto.AllUpdaters)).Returns(new AllUpdaters() { });

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>())).Returns(Task.FromResult<User?>(new User() { UserPrivileges = new UserPrivileges() { } }));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.IsAny<User>())).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateAllUpdaters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task api_user_privileges_update_deleters()
    {
        ObjectId id = ObjectId.GenerateNewId();

        UserPrivileges userPrivileges = new();
        UserPrivilegesPatchDto userPrivilegesDto = new() { Deleters = new DeleterPatchDto[] { new() { } } };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        ControllerFixture.IAuthHelper.Setup<Task<string?>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)).Returns(Task.FromResult<string?>(id.ToString()));

        ControllerFixture.IMapper.Setup<Deleter[]>(im => im.Map<Deleter[]>(userPrivilegesDto.Deleters)).Returns(new Deleter[] { });

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>())).Returns(Task.FromResult<User?>(new User() { UserPrivileges = new UserPrivileges() { } }));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateUserPrivileges(It.IsAny<User>())).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().UpdateDeleters(userPrivilegesDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }
}