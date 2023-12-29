namespace user_management_unit_tests;

using AutoMapper;
using MongoDB.Driver;
using Moq;
using user_management.Authentication;
using user_management.Authentication.JWT;
using user_management.Services.Data.Client;
using user_management.Services.Data.User;
using user_management.Utilities;


public class ServiceFixture
{
    public Mock<IAuthenticated> IAuthenticated { get; private set; } = new();
    public Mock<IAuthenticatedByJwt> IAuthenticatedByJwt { get; private set; } = new();
    public Mock<IAuthenticatedByBearer> IAuthenticatedByBearer { get; private set; } = new();

    public Mock<IUserRepository> IUserRepository { get; private set; } = new();
    public Mock<IClientRepository> IClientRepository { get; private set; } = new();

    public Mock<IMapper> IMapper { get; private set; } = new();
    public Mock<IStringHelper> IStringHelper { get; private set; } = new();
    public Mock<INotificationHelper> INotificationHelper { get; private set; } = new();
    public Mock<IDateTimeProvider> IDateTimeProvider { get; private set; } = new();
    public Mock<IMongoClient> IMongoClient { get; internal set; } = new();
    public Mock<IJWTAuthenticationHandler> IJWTAuthenticationHandler { get; internal set; } = new();

    public Mock<IAuthHelper> IAuthHelper { get; private set; } = new();

    public void Reset()
    {
        IAuthenticated = new Mock<IAuthenticated>();
        IAuthenticatedByJwt = new Mock<IAuthenticatedByJwt>();
        IAuthenticatedByBearer = new Mock<IAuthenticatedByBearer>();

        IUserRepository = new Mock<IUserRepository>();
        IClientRepository = new Mock<IClientRepository>();

        IMapper = new Mock<IMapper>();
        IStringHelper = new Mock<IStringHelper>();
        INotificationHelper = new Mock<INotificationHelper>();
        IDateTimeProvider = new Mock<IDateTimeProvider>();
        IMongoClient = new Mock<IMongoClient>();
        IJWTAuthenticationHandler = new Mock<IJWTAuthenticationHandler>();

        IAuthHelper = new Mock<IAuthHelper>();
    }
}

[CollectionDefinition("Service")]
public class Service : ICollectionFixture<ServiceFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
