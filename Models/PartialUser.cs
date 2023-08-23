using System.Reflection;
namespace user_management.Models;

using System.Dynamic;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class PartialUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    public ObjectId Id { get; set; }

    [BsonElement(PRIVILEGES)]
    [BsonRequired]
    public Privilege[]? Privileges { get { return _privileges; } set { _privileges = value; _isPrivilegesTouched = true; } }
    public const string PRIVILEGES = "privileges";
    public Privilege[]? _privileges;
    private bool _isPrivilegesTouched = false;
    public bool IsPrivilegesTouched() => _isPrivilegesTouched;

    [BsonElement(USER_PRIVILEGES)]
    [BsonRequired]
    public UserPrivileges? UserPrivileges { get { return _userPrivileges; } set { _userPrivileges = value; _isUserPrivilegesTouched = true; } }
    public const string USER_PRIVILEGES = "user_privileges";
    public UserPrivileges? _userPrivileges;
    private bool _isUserPrivilegesTouched = false;
    public bool IsUserPrivilegesTouched() => _isUserPrivilegesTouched;

    [BsonElement(CLIENTS)]
    [BsonRequired]
    public UserClient[]? Clients { get { return _clients; } set { _clients = value; _isClientsTouched = true; } }
    public const string CLIENTS = "clients";
    public UserClient[]? _clients;
    private bool _isClientsTouched = false;
    public bool IsClientsTouched() => _isClientsTouched;

    [BsonElement(FIRST_NAME)]
    public string? FirstName { get { return _firstName; } set { _firstName = value; _isFirstNameTouched = true; } }
    public const string FIRST_NAME = "first_name";
    public string? _firstName;
    private bool _isFirstNameTouched = false;
    public bool IsFirstNameTouched() => _isFirstNameTouched;

    [BsonElement(MIDDLE_NAME)]
    public string? MiddleName { get { return _middleName; } set { _middleName = value; _isMiddleNameTouched = true; } }
    public const string MIDDLE_NAME = "middle_name";
    public string? _middleName;
    private bool _isMiddleNameTouched = false;
    public bool IsMiddleNameTouched() => _isMiddleNameTouched;

    [BsonElement(LAST_NAME)]
    public string? LastName { get { return _lastName; } set { _lastName = value; _isLastNameTouched = true; } }
    public const string LAST_NAME = "last_name";
    public string? _lastName;
    private bool _isLastNameTouched = false;
    public bool IsLastNameTouched() => _isLastNameTouched;

    [BsonElement(EMAIL)]
    [BsonRequired]
    public string? Email { get { return _email; } set { _email = value; _isEmailTouched = true; } }
    public const string EMAIL = "email";
    public string? _email;
    private bool _isEmailTouched = false;
    public bool IsEmailTouched() => _isEmailTouched;

    [BsonElement(PHONE_NUMBER)]
    public string? PhoneNumber { get { return _phoneNumber; } set { _phoneNumber = value; _isPhoneNumberTouched = true; } }
    public const string PHONE_NUMBER = "phone_number";
    public string? _phoneNumber;
    private bool _isPhoneNumberTouched = false;
    public bool IsPhoneNumberTouched() => _isPhoneNumberTouched;

    [BsonElement(USERNAME)]
    [BsonRequired]
    public string? Username { get { return _username; } set { _username = value; _isUsernameTouched = true; } }
    public const string USERNAME = "username";
    public string? _username;
    private bool _isUsernameTouched = false;
    public bool IsUsernameTouched() => _isUsernameTouched;

    [BsonElement(PASSWORD)]
    [BsonRequired]
    public string? Password { get { return _password; } set { _password = value; _isPasswordTouched = true; } }
    public const string PASSWORD = "password";
    public string? _password;
    private bool _isPasswordTouched = false;
    public bool IsPasswordTouched() => _isPasswordTouched;

    [BsonElement(VERIFICATION_SECRET)]
    [BsonRequired]
    public string? VerificationSecret { get { return _verificationSecret; } set { _verificationSecret = value; _isVerificationSecretTouched = true; } }
    public const string VERIFICATION_SECRET = "verification_secret";
    public string? _verificationSecret;
    private bool _isVerificationSecretTouched = false;
    public bool IsVerificationSecretTouched() => _isVerificationSecretTouched;

    [BsonElement(VERIFICATION_SECRET_UPDATED_AT)]
    [BsonRequired]
    public DateTime? VerificationSecretUpdatedAt { get { return _verificationSecretUpdatedAt; } set { _verificationSecretUpdatedAt = value; _isVerificationSecretUpdatedAtTouched = true; } }
    public const string VERIFICATION_SECRET_UPDATED_AT = "verification_secret_updated_at";
    public DateTime? _verificationSecretUpdatedAt;
    private bool _isVerificationSecretUpdatedAtTouched = false;
    public bool IsVerificationSecretUpdatedAtTouched() => _isVerificationSecretUpdatedAtTouched;

    [BsonElement(IS_VERIFIED)]
    [BsonRequired]
    public bool? IsVerified { get { return _isVerified; } set { _isVerified = value; _isIsVerifiedTouched = true; } }
    public const string IS_VERIFIED = "is_verified";
    public bool? _isVerified;
    private bool _isIsVerifiedTouched = false;
    public bool IsIsVerifiedTouched() => _isIsVerifiedTouched;

    [BsonElement(LOGGED_OUT_AT)]
    public DateTime? LoggedOutAt { get { return _loggedOutAt; } set { _loggedOutAt = value; _isLoggedOutAtTouched = true; } }
    public const string LOGGED_OUT_AT = "logged_out_at";
    public DateTime? _loggedOutAt;
    private bool _isLoggedOutAtTouched = false;
    public bool IsLoggedOutAtTouched() => _isLoggedOutAtTouched;

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; _isUpdatedAtTouched = true; } }
    public const string UPDATED_AT = "updated_at";
    public DateTime? _updatedAt;
    private bool _isUpdatedAtTouched = false;
    public bool IsUpdatedAtTouched() => _isUpdatedAtTouched;

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime? CreatedAt { get { return _createdAt; } set { _createdAt = value; _isCreatedAtTouched = true; } }
    public const string CREATED_AT = "created_at";
    public DateTime? _createdAt;
    private bool _isCreatedAtTouched = false;
    public bool IsCreatedAtTouched() => _isCreatedAtTouched;

    public static IEnumerable<object> GetReadable(IEnumerable<PartialUser> users)
    {
        if (users == null || users.Count() == 0) return new List<object>() { };

        IEnumerable<object> rawUsers = new object[] { };

        for (int i = 0; i < users.Count(); i++)
            rawUsers = rawUsers.Append(users.ElementAt(i).GetReadable());

        return rawUsers;
    }

    public object GetReadable()
    {
        List<System.Reflection.PropertyInfo> properties = this.GetType().GetProperties().ToList()
            .Where(p => p.CustomAttributes.ToList().Where(ca => ca.AttributeType == typeof(BsonElementAttribute) || ca.AttributeType == typeof(BsonIdAttribute)).Count() != 0)
            .ToList();

        var retrievableUser = new ExpandoObject() as IDictionary<string, object?>;

        properties.ForEach(p =>
        {
            CustomAttributeData customAttribute = p.CustomAttributes.First(ca => ca.AttributeType == typeof(BsonElementAttribute) || ca.AttributeType == typeof(BsonIdAttribute));

            string key = (customAttribute.AttributeType == typeof(BsonIdAttribute) ? "_id" : customAttribute.ConstructorArguments[0].Value as string)!;
            object? value = p.GetValue(this);

            if (key == "_id")
            {
                retrievableUser.Add(key, value);
                return;
            }

            var method = this.GetType().GetMethod("Is" + p.Name + "Touched");
            object? methodReturn = null;
            if (method != null) methodReturn = method.Invoke(this, new object?[] { });

            if (methodReturn != null && (bool)methodReturn) retrievableUser.Add(key, value);
        });

        return retrievableUser;
    }
}