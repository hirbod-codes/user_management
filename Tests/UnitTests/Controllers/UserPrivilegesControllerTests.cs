using System.Security.Claims;
using System.Net.Mail;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data;
using user_management.Services.Data.User;
using Xunit;
using Moq;
using System.Dynamic;

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

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateReaders(dto));
    }

    [Fact]
    public async void UpdateReaders_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateReaders(dto));
    }

    [Fact]
    public async void UpdateReaders_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateReaders(dto));
    }

    [Fact]
    public async void UpdateReaders_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateReaders(dto));
    }

    [Fact]
    public async void UpdateReaders_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateReaders(authorId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateReaders(dto));
    }

    [Fact]
    public async void UpdateAllReaders_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateAllReaders(dto));
    }

    [Fact]
    public async void UpdateAllReaders_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateAllReaders(dto));
    }

    [Fact]
    public async void UpdateAllReaders_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllReaders(dto));
    }

    [Fact]
    public async void UpdateAllReaders_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateAllReaders(dto));
    }

    [Fact]
    public async void UpdateAllReaders_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllReaders(authorId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateAllReaders(dto));
    }

    [Fact]
    public async void UpdateUpdaters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateUpdaters(dto));
    }

    [Fact]
    public async void UpdateUpdaters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateUpdaters(dto));
    }

    [Fact]
    public async void UpdateUpdaters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateUpdaters(dto));
    }

    [Fact]
    public async void UpdateUpdaters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateUpdaters(dto));
    }

    [Fact]
    public async void UpdateUpdaters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateUpdaters(authorId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateUpdaters(dto));
    }

    [Fact]
    public async void UpdateAllUpdaters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateAllUpdaters(dto));
    }

    [Fact]
    public async void UpdateAllUpdaters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateAllUpdaters(dto));
    }

    [Fact]
    public async void UpdateAllUpdaters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateAllUpdaters(dto));
    }

    [Fact]
    public async void UpdateAllUpdaters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateAllUpdaters(dto));
    }

    [Fact]
    public async void UpdateAllUpdaters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateAllUpdaters(authorId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateAllUpdaters(dto));
    }

    [Fact]
    public async void UpdateDeleters_Ok()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, dto));
        HttpAsserts.IsOk(await InstantiateController().UpdateDeleters(dto));
    }

    [Fact]
    public async void UpdateDeleters_Unauthorized()
    {
        UserPrivilegesPatchDto dto = new();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("Not JWT");
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateDeleters(dto));
    }

    [Fact]
    public async void UpdateDeleters_Unauthenticated()
    {
        UserPrivilegesPatchDto dto = new();
        string? authorId = null;

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto));

        authorId = "authorId";

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, dto)).Throws(new ArgumentException("authorId"));
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateDeleters(dto));
    }

    [Fact]
    public async void UpdateDeleters_NotFound()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().UpdateDeleters(dto));
    }

    [Fact]
    public async void UpdateDeleters_Problem()
    {
        UserPrivilegesPatchDto dto = new();
        string authorId = ObjectId.GenerateNewId().ToString();

        Fixture.IAuthHelper.Setup<string>(um => um.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");
        Fixture.IAuthHelper.Setup<Task<string?>>(um => um.GetIdentifier(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult<string?>(authorId));
        Fixture.IUserPrivilegesManagement.Setup<Task>(um => um.UpdateDeleters(authorId, dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateDeleters(dto));
    }
}