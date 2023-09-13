using System.Dynamic;
using Bogus;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data;
using user_management.Services.Data.User;

namespace user_management_tests.UnitTests.Services;

[Collection("Service")]
public class UserManagementTests
{
    public ServiceFixture Fixture { get; private set; }

    public UserManagementTests(ServiceFixture serviceFixture) => Fixture = serviceFixture;

    private UserManagement InstantiateService() => new UserManagement(Fixture.IDateTimeProvider.Object, Fixture.INotificationHelper.Object, Fixture.IStringHelper.Object, Fixture.IUserRepository.Object, Fixture.IMapper.Object, Fixture.IAuthHelper.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void FullNameExistenceCheck_Ok()
    {
        string firstName = Faker.Person.FirstName;
        string middleName = Faker.Person.FirstName;
        string lastName = Faker.Person.LastName;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByFullNameForExistenceCheck(firstName, middleName, lastName)).Returns(Task.FromResult<User?>(new User()));
        Assert.True(await InstantiateService().FullNameExistenceCheck(firstName, middleName, lastName));

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByFullNameForExistenceCheck(firstName, middleName, lastName)).Returns(Task.FromResult<User?>(null));
        Assert.False(await InstantiateService().FullNameExistenceCheck(firstName, middleName, lastName));

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByFullNameForExistenceCheck(null, null, null)).Throws<ArgumentException>();
        await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().FullNameExistenceCheck(null, null, null));
    }

    [Fact]
    public async void UsernameExistenceCheck_Ok()
    {
        string username = Faker.Person.UserName;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByUsernameForExistenceCheck(username)).Returns(Task.FromResult<User?>(new User()));
        Assert.True(await InstantiateService().UsernameExistenceCheck(username));

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByUsernameForExistenceCheck(username)).Returns(Task.FromResult<User?>(null));
        Assert.False(await InstantiateService().UsernameExistenceCheck(username));
    }

    [Fact]
    public async void EmailExistenceCheck_Ok()
    {
        string email = Faker.Person.Email;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByEmailForExistenceCheck(email)).Returns(Task.FromResult<User?>(new User()));
        Assert.True(await InstantiateService().EmailExistenceCheck(email));

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByEmailForExistenceCheck(email)).Returns(Task.FromResult<User?>(null));
        Assert.False(await InstantiateService().EmailExistenceCheck(email));
    }

    [Fact]
    public async void PhoneNumberExistenceCheck_Ok()
    {
        string phoneNumber = Faker.Person.Phone;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByPhoneNumberForExistenceCheck(phoneNumber)).Returns(Task.FromResult<User?>(new User()));
        Assert.True(await InstantiateService().PhoneNumberExistenceCheck(phoneNumber));

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveByPhoneNumberForExistenceCheck(phoneNumber)).Returns(Task.FromResult<User?>(null));
        Assert.False(await InstantiateService().PhoneNumberExistenceCheck(phoneNumber));
    }

    [Fact]
    public async void Register_Ok()
    {
        UserCreateDto dto = new() { Email = Faker.Internet.Email(), Password = Faker.Internet.Password() };
        string code = "code";
        User unverifiedUser = new() { Email = dto.Email };
        string hashedPassword = "hashedPassword";
        DateTime dateTime = DateTime.UtcNow;

        Fixture.IStringHelper.Setup<string>(o => o.GenerateRandomString(6)).Returns(code);
        Fixture.INotificationHelper.Setup<Task>(o => o.SendVerificationMessage(dto.Email, code));
        Fixture.IMapper.Setup<User>(o => o.Map<User>(dto)).Returns(unverifiedUser);
        Fixture.IStringHelper.Setup<string>(o => o.Hash(dto.Password)).Returns(hashedPassword);
        Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(dateTime);

        unverifiedUser.Password = hashedPassword;
        unverifiedUser.VerificationSecret = code;
        unverifiedUser.VerificationSecretUpdatedAt = dateTime;
        unverifiedUser.IsVerified = false;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.Create(unverifiedUser)).Returns(Task.FromResult<User?>(unverifiedUser));

        Assert.Equal<User>(unverifiedUser, await InstantiateService().Register(dto));
    }

    [Fact]
    public async void Register_NotOk()
    {
        UserCreateDto dto = new() { Email = Faker.Internet.Email(), Password = Faker.Internet.Password() };
        string code = "code";
        User unverifiedUser = new() { Email = dto.Email };
        string hashedPassword = "hashedPassword";
        DateTime dateTime = DateTime.UtcNow;

        Fixture.IStringHelper.Setup<string>(o => o.GenerateRandomString(6)).Returns(code);
        Fixture.INotificationHelper.Setup<Task>(o => o.SendVerificationMessage(dto.Email, code)).Throws<Exception>();
        await Assert.ThrowsAsync<SmtpFailureException>(async () => await InstantiateService().Register(dto));

        Fixture.IMapper.Setup<User>(o => o.Map<User>(dto)).Returns(unverifiedUser);
        Fixture.IStringHelper.Setup<string>(o => o.Hash(dto.Password)).Returns(hashedPassword);
        Fixture.INotificationHelper.Setup<Task>(o => o.SendVerificationMessage(dto.Email, code));
        Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(dateTime);

        unverifiedUser.Password = hashedPassword;
        unverifiedUser.VerificationSecret = code;
        unverifiedUser.VerificationSecretUpdatedAt = dateTime;
        unverifiedUser.IsVerified = false;

        Fixture.IUserRepository.Setup<Task<User?>>(o => o.Create(unverifiedUser)).Returns(Task.FromResult<User?>(null));
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Register(dto));
    }

    public static IEnumerable<object?[]> Activate_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new Activation() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    Password = "password"
                } ,
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                }
            }
        };

    [Theory]
    [MemberData(nameof(Activate_Ok_Data))]
    public async void Activate_Ok(Activation dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));

        user.IsVerified = true;
        await InstantiateService().Activate(dto);
        user.IsVerified = false;

        Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Verify((ObjectId)user.Id)).Returns(Task.FromResult<bool?>(true));

        await InstantiateService().Activate(dto);
    }

    public static IEnumerable<object?[]> Activate_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new Activation() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    Password = "password"
                } ,
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new Activation() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    Password = "password"
                } ,
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-6),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            },
            new object?[] {
                new Activation() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    Password = "password"
                } ,
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-7),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            },
            new object?[] {
                new Activation() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "another-code",
                    Password = "password"
                } ,
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            },
        };

    [Theory]
    [MemberData(nameof(Activate_NotOk_Data))]
    public async void Activate_NotOk(Activation dto, User? user, DateTime now)
    {
        if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Activate(dto));
            return;
        }
        else if (((DateTime)user.VerificationSecretUpdatedAt!).AddMinutes(6) <= now)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<VerificationCodeExpiredException>(async () => await InstantiateService().Activate(dto));
            return;
        }
        else if (user.VerificationSecret != dto.VerificationSecret)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<InvalidVerificationCodeException>(async () => await InstantiateService().Activate(dto));
            return;
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(false);
            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await InstantiateService().Activate(dto));

            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Verify((ObjectId)user.Id)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Activate(dto));

            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Verify((ObjectId)user.Id)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Activate(dto));
        }
    }

    public static IEnumerable<object?[]> ChangePassword_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    PasswordConfirmation = "password",
                    Password = "password"
                },
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                }
            }
        };

    [Theory]
    [MemberData(nameof(ChangePassword_Ok_Data))]
    public async void ChangePassword_Ok(ChangePassword dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(user));

        string hashedPassword = "hashedPassword";
        Fixture.IStringHelper.Setup<string>(o => o.Hash(dto.Password)).Returns(hashedPassword);

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePassword(dto.Email, hashedPassword)).Returns(Task.FromResult<bool?>(true));

        await InstantiateService().ChangePassword(dto);
    }

    public static IEnumerable<object?[]> ChangePassword_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    PasswordConfirmation = "another-password",
                    Password = "password"
                },
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    PasswordConfirmation = "password",
                    Password = "password"
                },
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    PasswordConfirmation = "password",
                    Password = "password"
                },
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-6),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                    PasswordConfirmation = "password",
                    Password = "password"
                },
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-7),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePassword() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code-another",
                    PasswordConfirmation = "password",
                    Password = "password"
                },
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = false,
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                    Password = "hashedPassword"
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangePassword_NotOk_Data))]
    public async void ChangePassword_NotOk(ChangePassword dto, User? user, DateTime now)
    {
        if (dto.Password != dto.PasswordConfirmation)
            await Assert.ThrowsAsync<PasswordConfirmationMismatchException>(async () => await InstantiateService().ChangePassword(dto));
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangePassword(dto));
        }
        else if (((DateTime)user.VerificationSecretUpdatedAt!).AddMinutes(6) <= now)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<VerificationCodeExpiredException>(async () => await InstantiateService().ChangePassword(dto));
        }
        else if (dto.VerificationSecret != user.VerificationSecret)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<InvalidVerificationCodeException>(async () => await InstantiateService().ChangePassword(dto));
        }
        else
        {
            string anotherHashedPassword = "anotherHashedPassword";

            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            Fixture.IStringHelper.Setup<string>(o => o.Hash(dto.Password)).Returns(anotherHashedPassword);

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePassword(dto.Email, "anotherHashedPassword")).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangePassword(dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePassword(dto.Email, "anotherHashedPassword")).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ChangePassword(dto));
        }
    }

    public static IEnumerable<object?[]> Login_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new Login() {
                    Email = Faker.Internet.Email(),
                    Password = "password"
                },
                new User() {
                    Id = ObjectId.GenerateNewId(),
                    IsVerified = true,
                    Password = "hashedPassword"
                }
            }
        };

    [Theory]
    [MemberData(nameof(Login_Ok_Data))]
    public async void Login_Ok(Login dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
        Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Login(user.Id)).Returns(Task.FromResult<bool?>(true));
        Fixture.IAuthHelper.Setup(o => o.GenerateAuthenticationJWT(user.Id.ToString())).Returns("jwt");

        var userObject = new ExpandoObject() as IDictionary<string, object>;
        userObject.Add("_id", user.Id.ToString()!);

        Assert.Equal("jwt", (await InstantiateService().Login(dto)).jwt);
        Assert.Equal<object>(user.Id.ToString(), (await InstantiateService().Login(dto)).userId);
    }

    public static IEnumerable<object?[]> Login_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new Login() {
                },
                null
            },
            new object?[] {
                new Login() {
                    Email = Faker.Internet.Email(),
                },
                null
            },
            new object?[] {
                new Login() {
                    Email = Faker.Internet.Email(),
                    Password = "wrongPassword"
                },
                new User() {
                    Password = "password",
                }
            },
            new object?[] {
                new Login() {
                    Email = Faker.Internet.Email(),
                    Password = "password"
                },
                new User() {
                    Password = "password",
                    IsVerified = false,
                }
            },
            new object?[] {
                new Login() {
                    Email = Faker.Internet.Email(),
                    Password = "password"
                },
                new User() {
                    Password = "password",
                    IsVerified = true,
                }
            },
        };

    [Theory]
    [MemberData(nameof(Login_NotOk_Data))]
    public async void Login_NotOk(Login dto, User user)
    {
        if (dto.Username == null && dto.Email == null)
            await Assert.ThrowsAsync<MissingCredentialException>(async () => await InstantiateService().Login(dto));
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Login(dto));
        }
        else if (user.Password != dto.Password)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(false);
            await Assert.ThrowsAsync<InvalidPasswordException>(async () => await InstantiateService().Login(dto));
        }
        else if (user.IsVerified == false)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);
            await Assert.ThrowsAsync<UnverifiedUserException>(async () => await InstantiateService().Login(dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserByLoginCredentials(dto.Email, null)).Returns(Task.FromResult<User?>(user));
            Fixture.IStringHelper.Setup<bool>(o => o.DoesHashMatch(user.Password, dto.Password)).Returns(true);

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Login(user.Id)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Login(dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Login(user.Id)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Login(dto));
        }
    }

    public static IEnumerable<object?[]> Logout_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString()
            }
        };

    [Theory]
    [MemberData(nameof(Logout_Ok_Data))]
    public async void Logout_Ok(string identifier)
    {
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Logout(ObjectId.Parse(identifier))).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().Logout(identifier);
    }

    public static IEnumerable<object?[]> Logout_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id"
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString()
            }
        };

    [Theory]
    [MemberData(nameof(Logout_NotOk_Data))]
    public async void Logout_NotOk(string identifier)
    {
        if (identifier == "id") await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Logout(identifier));
        else
        {
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Logout(ObjectId.Parse(identifier))).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Logout(identifier));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Logout(ObjectId.Parse(identifier))).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Logout(identifier));
        }
    }

    public static IEnumerable<object?[]> ChangeUsername_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangeUsername() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangeUsername_Ok_Data))]
    public async void ChangeUsername_Ok(ChangeUsername dto, User user, DateTime now)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForUsernameChange(dto.Email)).Returns(Task.FromResult<User?>(user));
        Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeUsername(dto.Email, dto.Username)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().ChangeUsername(dto);
    }

    public static IEnumerable<object?[]> ChangeUsername_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangeUsername() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeUsername() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-6),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeUsername() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-7),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeUsername() {
                    Email = Faker.Internet.Email(),
                    VerificationSecret = "code-another",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangeUsername_NotOk_Data))]
    public async void ChangeUsername_NotOk(ChangeUsername dto, User? user, DateTime now)
    {
        if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForUsernameChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangeUsername(dto));
        }
        else if (((DateTime)user.VerificationSecretUpdatedAt!).AddMinutes(6) <= now)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForUsernameChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<VerificationCodeExpiredException>(async () => await InstantiateService().ChangeUsername(dto));
        }
        else if (dto.VerificationSecret != user.VerificationSecret)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForUsernameChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<InvalidVerificationCodeException>(async () => await InstantiateService().ChangeUsername(dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForUsernameChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeUsername(dto.Email, dto.Username)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangeUsername(dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeUsername(dto.Email, dto.Username)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ChangeUsername(dto));
        }
    }

    public static IEnumerable<object?[]> ChangeEmail_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangeEmail() {
                    Email = Faker.Internet.Email(),
                    NewEmail = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangeEmail_Ok_Data))]
    public async void ChangeEmail_Ok(ChangeEmail dto, User user, DateTime now)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForEmailChange(dto.Email)).Returns(Task.FromResult<User?>(user));
        Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeEmail(dto.Email, dto.NewEmail)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().ChangeEmail(dto);
    }

    public static IEnumerable<object?[]> ChangeEmail_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangeEmail() {
                    Email = Faker.Internet.Email(),
                    NewEmail = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeEmail() {
                    Email = Faker.Internet.Email(),
                    NewEmail = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-6),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeEmail() {
                    Email = Faker.Internet.Email(),
                    NewEmail = Faker.Internet.Email(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-7),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangeEmail() {
                    Email = Faker.Internet.Email(),
                    NewEmail = Faker.Internet.Email(),
                    VerificationSecret = "code-another",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangeEmail_NotOk_Data))]
    public async void ChangeEmail_NotOk(ChangeEmail dto, User? user, DateTime now)
    {
        if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForEmailChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangeEmail(dto));
        }
        else if (((DateTime)user.VerificationSecretUpdatedAt!).AddMinutes(6) <= now)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForEmailChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<VerificationCodeExpiredException>(async () => await InstantiateService().ChangeEmail(dto));
        }
        else if (dto.VerificationSecret != user.VerificationSecret)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForEmailChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<InvalidVerificationCodeException>(async () => await InstantiateService().ChangeEmail(dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForEmailChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeEmail(dto.Email, dto.NewEmail)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangeEmail(dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangeEmail(dto.Email, dto.NewEmail)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ChangeEmail(dto));
        }
    }

    public static IEnumerable<object?[]> ChangePhoneNumber_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangePhoneNumber() {
                    Email = Faker.Internet.Email(),
                    PhoneNumber = Faker.Phone.PhoneNumber(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangePhoneNumber_Ok_Data))]
    public async void ChangePhoneNumber_Ok(ChangePhoneNumber dto, User user, DateTime now)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPhoneNumberChange(dto.Email)).Returns(Task.FromResult<User?>(user));
        Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePhoneNumber(dto.Email, dto.PhoneNumber)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().ChangePhoneNumber(dto);
    }

    public static IEnumerable<object?[]> ChangePhoneNumber_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                new ChangePhoneNumber() {
                    Email = Faker.Internet.Email(),
                    PhoneNumber = Faker.Phone.PhoneNumber(),
                    VerificationSecret = "code",
                },
                null,
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePhoneNumber() {
                    Email = Faker.Internet.Email(),
                    PhoneNumber = Faker.Phone.PhoneNumber(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-6),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePhoneNumber() {
                    Email = Faker.Internet.Email(),
                    PhoneNumber = Faker.Phone.PhoneNumber(),
                    VerificationSecret = "code",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-7),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            },
            new object?[] {
                new ChangePhoneNumber() {
                    Email = Faker.Internet.Email(),
                    PhoneNumber = Faker.Phone.PhoneNumber(),
                    VerificationSecret = "code-another",
                },
                new User() {
                    VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                    VerificationSecret = "code",
                },
                DateTime.UtcNow
            }
        };

    [Theory]
    [MemberData(nameof(ChangePhoneNumber_NotOk_Data))]
    public async void ChangePhoneNumber_NotOk(ChangePhoneNumber dto, User? user, DateTime now)
    {
        if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPhoneNumberChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangePhoneNumber(dto));
        }
        else if (((DateTime)user.VerificationSecretUpdatedAt!).AddMinutes(6) <= now)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPhoneNumberChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<VerificationCodeExpiredException>(async () => await InstantiateService().ChangePhoneNumber(dto));
        }
        else if (dto.VerificationSecret != user.VerificationSecret)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPhoneNumberChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);
            await Assert.ThrowsAsync<InvalidVerificationCodeException>(async () => await InstantiateService().ChangePhoneNumber(dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveUserForPhoneNumberChange(dto.Email)).Returns(Task.FromResult<User?>(user));
            Fixture.IDateTimeProvider.Setup<DateTime>(o => o.ProvideUtcNow()).Returns(now);

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePhoneNumber(dto.Email, dto.PhoneNumber)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ChangePhoneNumber(dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.ChangePhoneNumber(dto.Email, dto.PhoneNumber)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ChangePhoneNumber(dto));
        }
    }

    public static IEnumerable<object?[]> RemoveClient_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            }
        };

    [Theory]
    [MemberData(nameof(RemoveClient_Ok_Data))]
    public async void RemoveClient_Ok(string clientId, string userId, string authorId, bool forClients)
    {
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveClient(ObjectId.Parse(userId), ObjectId.Parse(clientId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().RemoveClient(clientId, userId, authorId, forClients);
    }

    public static IEnumerable<object?[]> RemoveClient_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                ObjectId.GenerateNewId().ToString(),
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                "id",
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            },
        };

    [Theory]
    [MemberData(nameof(RemoveClient_NotOk_Data))]
    public async void RemoveClient_NotOk(string clientId, string userId, string authorId, bool forClients)
    {
        if (userId == "id" || clientId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RemoveClient(clientId, userId, authorId, forClients));
            if (clientId == "id") Assert.Equal("clientId", ex.Message);
            if (authorId == "id") Assert.Equal("authorId", ex.Message);
            if (userId == "id") Assert.Equal("userId", ex.Message);
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveClient(ObjectId.Parse(userId), ObjectId.Parse(clientId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().RemoveClient(clientId, userId, authorId, forClients));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveClient(ObjectId.Parse(userId), ObjectId.Parse(clientId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().RemoveClient(clientId, userId, authorId, forClients));
        }
    }

    public static IEnumerable<object?[]> RemoveClients_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            }
        };

    [Theory]
    [MemberData(nameof(RemoveClients_Ok_Data))]
    public async void RemoveClients_Ok(string userId, string authorId, bool forClients)
    {
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveAllClients(ObjectId.Parse(userId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().RemoveClients(userId, authorId, forClients);
    }

    public static IEnumerable<object?[]> RemoveClients_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                false
            },
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            }
        };

    [Theory]
    [MemberData(nameof(RemoveClients_NotOk_Data))]
    public async void RemoveClients_NotOk(string userId, string authorId, bool forClients)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RemoveClients(userId, authorId, forClients));
            if (authorId == "id") Assert.Equal("authorId", ex.Message);
            else Assert.Equal("userId", ex.Message);
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveAllClients(ObjectId.Parse(userId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().RemoveClients(userId, authorId, forClients));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.RemoveAllClients(ObjectId.Parse(userId), ObjectId.Parse(authorId), forClients)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().RemoveClients(userId, authorId, forClients));
        }
    }

    public static IEnumerable<object?[]> RetrieveById_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false,
                new PartialUser()
            }
        };

    [Theory]
    [MemberData(nameof(RetrieveById_Ok_Data))]
    public async void RetrieveById_Ok(string actorId, string userId, bool forClients, PartialUser user)
    {
        Fixture.IUserRepository.Setup<Task<PartialUser?>>(o => o.RetrieveById(ObjectId.Parse(actorId), ObjectId.Parse(userId), forClients)).Returns(Task.FromResult<PartialUser?>(user));
        Assert.Equal<PartialUser>(user, await InstantiateService().RetrieveById(actorId, userId, forClients));
    }

    public static IEnumerable<object?[]> RetrieveById_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                false,
                new PartialUser()
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                false,
                new PartialUser()
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false,
                null
            }
        };

    [Theory]
    [MemberData(nameof(RetrieveById_NotOk_Data))]
    public async void RetrieveById_NotOk(string actorId, string userId, bool forClients, PartialUser? user)
    {
        if (actorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RetrieveById(actorId, userId, forClients));
            Assert.Equal("actorId", ex.Message);
        }
        else if (userId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RetrieveById(actorId, userId, forClients));
            Assert.Equal("userId", ex.Message);
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<PartialUser?>>(o => o.RetrieveById(ObjectId.Parse(actorId), ObjectId.Parse(userId), false)).Returns(Task.FromResult<PartialUser?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().RetrieveById(actorId, userId, forClients));
        }
    }

    public static IEnumerable<object?[]> Retrieve_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                new List<PartialUser>(),
                ObjectId.GenerateNewId().ToString(),
                false,
                "",
                0,
                0,
                null
            }
        };

    [Theory]
    [MemberData(nameof(Retrieve_Ok_Data))]
    public async void Retrieve_Ok(List<PartialUser> users, string actorId, bool forClients, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        Fixture.IUserRepository.Setup<Task<List<PartialUser>>>(o => o.Retrieve(ObjectId.Parse(actorId), logicsString, limit, iteration, sortBy, ascending, forClients)).Returns(Task.FromResult<List<PartialUser>>(users));
        Assert.Equal<List<PartialUser>>(users, await InstantiateService().Retrieve(actorId, forClients, logicsString, limit, iteration, sortBy, ascending));
    }

    public static IEnumerable<object?[]> Retrieve_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                false,
                "",
                0,
                0,
                null
            }
        };

    [Theory]
    [MemberData(nameof(Retrieve_NotOk_Data))]
    public async void Retrieve_NotOk(string actorId, bool forClients, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        if (actorId == "id") await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Retrieve(actorId, forClients, logicsString, limit, iteration, sortBy, ascending));
    }

    public static IEnumerable<object?[]> Update_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPatchDto() { FiltersString = "filter", UpdatesString = "update" },
                false
            }
        };

    [Theory]
    [MemberData(nameof(Update_Ok_Data))]
    public async void Update_Ok(string actorId, UserPatchDto dto, bool forClients)
    {
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Update(ObjectId.Parse(actorId), dto.FiltersString!, dto.UpdatesString!, forClients)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().Update(actorId, dto, forClients);
    }

    public static IEnumerable<object?[]> Update_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPatchDto() { UpdatesString = "update" },
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPatchDto() { FiltersString = "filter" },
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPatchDto() { FiltersString = "empty" },
                false
            },
            new object?[] {
                "id",
                new UserPatchDto() {  },
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPatchDto() { FiltersString = "filter", UpdatesString = "update" },
                false
            }
        };

    [Theory]
    [MemberData(nameof(Update_NotOk_Data))]
    public async void Update_NotOk(string actorId, UserPatchDto dto, bool forClients)
    {
        if (dto.UpdatesString == null || dto.FiltersString == null || dto.FiltersString == "empty")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Update(actorId, dto, forClients));
            Assert.Equal("userPatchDto", ex.Message);
        }
        else if (actorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Update(actorId, dto, forClients));
            Assert.Equal("actorId", ex.Message);
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Update(ObjectId.Parse(actorId), dto.FiltersString!, dto.UpdatesString!, forClients)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Update(actorId, dto, forClients));
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Update(ObjectId.Parse(actorId), dto.FiltersString!, dto.UpdatesString!, forClients)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Update(actorId, dto, forClients));
        }
    }

    public static IEnumerable<object?[]> Delete_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            }
        };

    [Theory]
    [MemberData(nameof(Delete_Ok_Data))]
    public async void Delete_Ok(string actorId, string userId, bool forClients)
    {
        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Delete(ObjectId.Parse(actorId), ObjectId.Parse(userId), forClients)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().Delete(actorId, userId, forClients);
    }

    public static IEnumerable<object?[]> Delete_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                false
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                false
            }
        };

    [Theory]
    [MemberData(nameof(Delete_NotOk_Data))]
    public async void Delete_NotOk(string actorId, string userId, bool forClients)
    {
        if (actorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Delete(actorId, userId, forClients));
            Assert.Equal("actorId", ex.Message);
        }
        else if (userId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Delete(actorId, userId, forClients));
            Assert.Equal("userId", ex.Message);
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Delete(ObjectId.Parse(actorId), ObjectId.Parse(userId), forClients)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Delete(actorId, userId, forClients));
            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.Delete(ObjectId.Parse(actorId), ObjectId.Parse(userId), forClients)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().Delete(actorId, userId, forClients));
        }
    }
}