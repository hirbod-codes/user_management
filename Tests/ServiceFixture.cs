namespace user_management.Tests;

using AutoMapper;
using MongoDB.Driver;
using Moq;
using user_management.Authentication.JWT;
using user_management.Services.Client;
using user_management.Services.Data.User;
using user_management.Utilities;
using Xunit;

public class ServiceFixture
{
    public Mock<IUserRepository> IUserRepository { get; private set; } = new();
    public Mock<IClientRepository> IClientRepository { get; private set; } = new();

    public Mock<IMapper> IMapper { get; private set; } = new();
    public Mock<IStringHelper> IStringHelper { get; private set; } = new();
    public Mock<INotificationHelper> INotificationHelper { get; private set; } = new();
    public Mock<IDateTimeProvider> IDateTimeProvider { get; private set; } = new();
    public Mock<IMongoClient> IMongoClient { get; internal set; } = new();
    public Mock<IJWTAuthenticationHandler> IJWTAuthenticationHandler { get; internal set; } = new();

    public void Reset()
    {
        IUserRepository = new Mock<IUserRepository>();
        IClientRepository = new Mock<IClientRepository>();

        IMapper = new Mock<IMapper>();
        IStringHelper = new Mock<IStringHelper>();
        INotificationHelper = new Mock<INotificationHelper>();
        IDateTimeProvider = new Mock<IDateTimeProvider>();
        IMongoClient = new Mock<IMongoClient>();
        IJWTAuthenticationHandler = new Mock<IJWTAuthenticationHandler>();
    }
}

[CollectionDefinition("Service")]
public class Service : ICollectionFixture<ServiceFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}