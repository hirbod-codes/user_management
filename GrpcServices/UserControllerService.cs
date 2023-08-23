namespace user_management.GrpcServices;

using AutoMapper;
using Grpc.Core;
using MongoDB.Bson;
using Newtonsoft.Json;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data.User;
using user_management.Utilities;

public class UserControllerService : UserController.UserControllerBase
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly UserManagement _userManagement;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAuthHelper _authHelper;
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserControllerService(IUserRepository userRepository, IMapper mapper, IStringHelper stringHelper, INotificationHelper notificationHelper, IAuthHelper authHelper, IDateTimeProvider dateTimeProvider, UserManagement userManagement)
    {
        _userManagement = userManagement;
        _mapper = mapper;
        _userRepository = userRepository;
        _authHelper = authHelper;
        _notificationHelper = notificationHelper;
        _stringHelper = stringHelper;
        _dateTimeProvider = dateTimeProvider;
    }
    public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
    {
        PartialUser user = await _userManagement.RetrieveById(request.AuthorId, request.Id, request.IsClient);

        if (!ObjectId.TryParse(request.AuthorId, out ObjectId authorObjectId)) throw new ArgumentException();

        object? content = user.GetReadable();
        return new GetResponse()
        {
            User = new Google.Protobuf.WellKnownTypes.Any()
            {
                TypeUrl = "#",
                Value = Google.Protobuf.ByteString.CopyFrom(Convert.FromBase64String((new StringHelper()).Base64Encode(JsonConvert.SerializeObject(content))))
            }
        };
    }
}