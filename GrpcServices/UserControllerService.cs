namespace user_management.GrpcServices;

using AutoMapper;
using Grpc.Core;
using user_management.Services.Data.User;
using user_management.Utilities;

public class UserControllerService : UserController.UserControllerBase
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAuthHelper _authHelper;
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserControllerService(IUserRepository userRepository, IMapper mapper, IStringHelper stringHelper, INotificationHelper notificationHelper, IAuthHelper authHelper, IDateTimeProvider dateTimeProvider)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _authHelper = authHelper;
        _notificationHelper = notificationHelper;
        _stringHelper = stringHelper;
        _dateTimeProvider = dateTimeProvider;
    }
    public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetResponse() { User = new Google.Protobuf.WellKnownTypes.Any() { TypeUrl = "#", Value = Google.Protobuf.ByteString.CopyFrom(Convert.FromBase64String((new StringHelper()).Base64Encode("content"))) } });
    }
}