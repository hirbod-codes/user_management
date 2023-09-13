namespace user_management.GrpcServices;

using Grpc.Core;
using MongoDB.Bson;
using Newtonsoft.Json;
using user_management.Models;
using user_management.Services;
using user_management.Utilities;

public class UserControllerService : UserController.UserControllerBase
{
    private readonly UserManagement _userManagement;
    private readonly IStringHelper _stringHelper;

    public UserControllerService(IStringHelper stringHelper, UserManagement userManagement)
    {
        _userManagement = userManagement;
        _stringHelper = stringHelper;
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
                Value = Google.Protobuf.ByteString.CopyFrom(Convert.FromBase64String(_stringHelper.Base64Encode(JsonConvert.SerializeObject(content))))
            }
        };
    }
}