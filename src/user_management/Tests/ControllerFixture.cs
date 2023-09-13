namespace user_management.Tests;

using AutoMapper;
using Moq;
using user_management.Authentication;
using user_management.Controllers.Services;
using user_management.Utilities;
using Xunit;

public class ControllerFixture
{
    public Mock<IAuthenticated> IAuthenticated { get; private set; } = new();
    public Mock<IAuthenticatedByJwt> IAuthenticatedByJwt { get; private set; } = new();
    public Mock<IAuthenticatedByBearer> IAuthenticatedByBearer { get; private set; } = new();

    public Mock<IUserManagement> IUserManagement { get; private set; } = new();
    public Mock<IUserPrivilegesManagement> IUserPrivilegesManagement { get; private set; } = new();
    public Mock<IClientManagement> IClientManagement { get; private set; } = new();
    public Mock<ITokenManagement> ITokenManagement { get; private set; } = new();

    public Mock<IAuthHelper> IAuthHelper { get; private set; } = new();
    public Mock<IMapper> IMapper { get; private set; } = new();

    public void Reset()
    {
        IAuthenticated = new Mock<IAuthenticated>();
        IAuthenticatedByJwt = new Mock<IAuthenticatedByJwt>();
        IAuthenticatedByBearer = new Mock<IAuthenticatedByBearer>();

        IUserManagement = new Mock<IUserManagement>();
        IUserPrivilegesManagement = new Mock<IUserPrivilegesManagement>();
        IClientManagement = new Mock<IClientManagement>();
        ITokenManagement = new Mock<ITokenManagement>();

        IAuthHelper = new Mock<IAuthHelper>();
        IMapper = new Mock<IMapper>();
    }
}

[CollectionDefinition("Controller")]
public class Controller : ICollectionFixture<ControllerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}