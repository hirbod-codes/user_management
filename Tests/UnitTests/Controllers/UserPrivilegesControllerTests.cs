using System.Security.Claims;
using Bogus;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Services;
using user_management.Services.Data;
using Xunit;
using Moq;

namespace user_management.Tests.UnitTests.Controllers;

[Collection("Controller")]
public class UserPrivilegesControllerTests
{
    public ControllerFixture Fixture { get; private set; }

    public UserPrivilegesControllerTests(ControllerFixture controllerFixture) => Fixture = controllerFixture;

    private user_management.Controllers.UserPrivilegesController InstantiateController() => new user_management.Controllers.UserPrivilegesController(Fixture.IAuthHelper.Object, Fixture.IUserPrivilegesManagement.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void UpdateReaders_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, userId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateReaders(dto, userId));
    }

    [Fact]
    public async void UpdateReaders_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateReaders(dto, userId));
    }

    [Fact]
    public async void UpdateReaders_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, userId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto, userId));
    }

    [Fact]
    public async void UpdateReaders_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, userId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateReaders(dto, userId));
    }

    [Fact]
    public async void UpdateReaders_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, userId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateReaders(dto, userId));
    }

    [Fact]
    public async void UpdateAllReaders_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, userId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateAllReaders(dto, userId));
    }

    [Fact]
    public async void UpdateAllReaders_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateAllReaders(dto, userId));
    }

    [Fact]
    public async void UpdateAllReaders_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, userId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto, userId));
    }

    [Fact]
    public async void UpdateAllReaders_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, userId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateAllReaders(dto, userId));
    }

    [Fact]
    public async void UpdateAllReaders_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, userId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateAllReaders(dto, userId));
    }

    [Fact]
    public async void UpdateUpdaters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, userId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateUpdaters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateUpdaters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, userId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateUpdaters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, userId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateUpdaters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, userId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateAllUpdaters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, userId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateAllUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateAllUpdaters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateAllUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateAllUpdaters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, userId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateAllUpdaters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, userId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateAllUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateAllUpdaters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, userId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateAllUpdaters(dto, userId));
    }

    [Fact]
    public async void UpdateDeleters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, userId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateDeleters(dto, userId));
    }

    [Fact]
    public async void UpdateDeleters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateDeleters(dto, userId));
    }

    [Fact]
    public async void UpdateDeleters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto, userId));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, userId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto, userId));
    }

    [Fact]
    public async void UpdateDeleters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, userId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateDeleters(dto, userId));
    }

    [Fact]
    public async void UpdateDeleters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();
        string userId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, userId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateDeleters(dto, userId));
    }
}