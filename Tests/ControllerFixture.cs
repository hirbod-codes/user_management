namespace user_management.Tests;

using AutoMapper;
using MongoDB.Driver;
using Moq;
using user_management.Controllers.Services;
using user_management.Services.Client;
using user_management.Services.Data.User;
using user_management.Utilities;
using Xunit;

public class ControllerFixture
{
    public Mock<IUserManagement> IUserManagement { get; private set; } = new();

    public Mock<IUserRepository> IUserRepository { get; private set; } = new();
    public Mock<IClientRepository> IClientRepository { get; private set; } = new();

    public Mock<IMapper> IMapper { get; private set; } = new();
    public Mock<IStringHelper> IStringHelper { get; private set; } = new();
    public Mock<IAuthHelper> IAuthHelper { get; private set; } = new();
    public Mock<INotificationHelper> INotificationHelper { get; private set; } = new();
    public Mock<IDateTimeProvider> IDateTimeProvider { get; private set; } = new();
    public Mock<IMongoClient> IMongoClient { get; internal set; } = new();

    public void Reset()
    {
        IUserManagement = new Mock<IUserManagement>();

        IUserRepository = new Mock<IUserRepository>();
        IClientRepository = new Mock<IClientRepository>();

        IMapper = new Mock<IMapper>();
        IStringHelper = new Mock<IStringHelper>();
        IAuthHelper = new Mock<IAuthHelper>();
        INotificationHelper = new Mock<INotificationHelper>();
        IDateTimeProvider = new Mock<IDateTimeProvider>();
        IMongoClient = new Mock<IMongoClient>();
    }
}

[CollectionDefinition("Controller")]
public class Controller : ICollectionFixture<ControllerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}