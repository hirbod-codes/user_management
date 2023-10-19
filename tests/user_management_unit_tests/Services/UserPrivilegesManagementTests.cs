using Bogus;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data;

namespace user_management_unit_tests.Services;

[Collection("Service")]
public class UserPrivilegesManagementTests
{
    public ServiceFixture Fixture { get; private set; }

    public UserPrivilegesManagementTests(ServiceFixture serviceFixture) => Fixture = serviceFixture;

    private UserPrivilegesManagement InstantiateService() => new UserPrivilegesManagement(Fixture.IUserRepository.Object, Fixture.IMapper.Object);

    public static Faker Faker = new("en");

    public static IEnumerable<object?[]> UpdateReaders_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Readers = new ReaderPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Readers = new Reader[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateReaders_Ok_Data))]
    public async void UpdateReaders_Ok(string authorId, string userId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

        List<Reader> mappedReaders = new() { };
        for (int i = 0; i < dto.Readers!.Length; i++)
        {
            Reader mappedReader = new Reader() { };
            Fixture.IMapper.Setup<Reader>(o => o.Map<Reader>(dto.Readers[i])).Returns(mappedReader);
            mappedReaders.Add(mappedReader);

        }
        user.UserPermissions.Readers = mappedReaders.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateReaders(authorId, userId, dto);
    }

    public static IEnumerable<object?[]> UpdateReaders_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Readers = new ReaderPatchDto[] { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Readers = new ReaderPatchDto[] { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Readers = new ReaderPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Readers = new Reader[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateReaders_NotOk_Data))]
    public async void UpdateReaders_NotOk(string authorId, string userId, UserPrivilegesPatchDto dto, User? user)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));
            if (userId == "id") Assert.Equal("userId", ex.Message);
            else Assert.Equal("authorId", ex.Message);
        }
        else if (dto.Readers == null)
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));
            Assert.Equal("dto", ex.Message);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

            List<Reader> mappedReaders = new() { };
            for (int i = 0; i < dto.Readers!.Length; i++)
            {
                Reader mappedReader = new Reader() { };
                Fixture.IMapper.Setup<Reader>(o => o.Map<Reader>(dto.Readers[i])).Returns(mappedReader);
                mappedReaders.Add(mappedReader);

            }
            user!.UserPermissions.Readers = mappedReaders.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateReaders(authorId, userId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateAllReaders_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllReaders = new AllReaders() { } },
                new User() { UserPermissions = new UserPermissions() { AllReaders = new AllReaders() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllReaders_Ok_Data))]
    public async void UpdateAllReaders_Ok(string authorId, string userId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

        AllReaders mappedAllReaders = new() { };
        Fixture.IMapper.Setup<AllReaders>(o => o.Map<AllReaders>(dto.AllReaders)).Returns(mappedAllReaders);
        user.UserPermissions.AllReaders = mappedAllReaders;

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateAllReaders(authorId, userId, dto);
    }

    public static IEnumerable<object?[]> UpdateAllReaders_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllReaders = new AllReaders() { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllReaders = new AllReaders() { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllReaders = new AllReaders() { } },
                new User() { UserPermissions = new UserPermissions() { AllReaders = new AllReaders() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllReaders_NotOk_Data))]
    public async void UpdateAllReaders_NotOk(string authorId, string userId, UserPrivilegesPatchDto dto, User? user)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
            if (userId == "id") Assert.Equal("userId", ex.Message);
            else Assert.Equal("authorId", ex.Message);
        }
        else if (dto.AllReaders == null)
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
            Assert.Equal("dto", ex.Message);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

            AllReaders mappedAllReaders = new() { };
            Fixture.IMapper.Setup<AllReaders>(o => o.Map<AllReaders>(dto.AllReaders)).Returns(mappedAllReaders);
            user!.UserPermissions.AllReaders = mappedAllReaders;

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateUpdaters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Updaters = new UpdaterPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Updaters = new Updater[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateUpdaters_Ok_Data))]
    public async void UpdateUpdaters_Ok(string authorId, string userId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

        List<Updater> mappedUpdaters = new() { };
        for (int i = 0; i < dto.Updaters!.Length; i++)
        {
            Updater mappedUpdater = new Updater() { };
            Fixture.IMapper.Setup<Updater>(o => o.Map<Updater>(dto.Updaters[i])).Returns(mappedUpdater);
            mappedUpdaters.Add(mappedUpdater);

        }
        user.UserPermissions.Updaters = mappedUpdaters.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateUpdaters(authorId, userId, dto);
    }

    public static IEnumerable<object?[]> UpdateUpdaters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Updaters = new UpdaterPatchDto[] { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Updaters = new UpdaterPatchDto[] { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Updaters = new UpdaterPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Updaters = new Updater[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateUpdaters_NotOk_Data))]
    public async void UpdateUpdaters_NotOk(string authorId, string userId, UserPrivilegesPatchDto dto, User? user)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
            if (userId == "id") Assert.Equal("userId", ex.Message);
            else Assert.Equal("authorId", ex.Message);
        }
        else if (dto.Updaters == null)
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateUpdaters(authorId, userId, dto));
            Assert.Equal("dto", ex.Message);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateUpdaters(authorId, userId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateUpdaters(authorId, userId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

            List<Updater> mappedUpdaters = new() { };
            for (int i = 0; i < dto.Updaters!.Length; i++)
            {
                Updater mappedUpdater = new Updater() { };
                Fixture.IMapper.Setup<Updater>(o => o.Map<Updater>(dto.Updaters[i])).Returns(mappedUpdater);
                mappedUpdaters.Add(mappedUpdater);

            }
            user!.UserPermissions.Updaters = mappedUpdaters.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateUpdaters(authorId, userId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateUpdaters(authorId, userId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateAllUpdaters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllUpdaters = new AllUpdaters() { } },
                new User() { UserPermissions = new UserPermissions() { AllUpdaters = new AllUpdaters() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllUpdaters_Ok_Data))]
    public async void UpdateAllUpdaters_Ok(string authorId, string userId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

        AllUpdaters mappedAllReaders = new() { };
        Fixture.IMapper.Setup<AllUpdaters>(o => o.Map<AllUpdaters>(dto.AllUpdaters)).Returns(mappedAllReaders);
        user.UserPermissions.AllUpdaters = mappedAllReaders;

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateAllUpdaters(authorId, userId, dto);
    }

    public static IEnumerable<object?[]> UpdateAllUpdaters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllUpdaters = new AllUpdaters() { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllUpdaters = new AllUpdaters() { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { AllUpdaters = new AllUpdaters() { } },
                new User() { UserPermissions = new UserPermissions() { AllUpdaters = new AllUpdaters() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllUpdaters_NotOk_Data))]
    public async void UpdateAllUpdaters_NotOk(string authorId, string userId, UserPrivilegesPatchDto dto, User? user)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
            if (userId == "id") Assert.Equal("userId", ex.Message);
            else Assert.Equal("authorId", ex.Message);
        }
        else if (dto.AllUpdaters == null)
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, userId, dto));
            Assert.Equal("dto", ex.Message);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, userId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, userId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

            AllUpdaters mappedAllReaders = new() { };
            Fixture.IMapper.Setup<AllUpdaters>(o => o.Map<AllUpdaters>(dto.AllUpdaters)).Returns(mappedAllReaders);
            user!.UserPermissions.AllUpdaters = mappedAllReaders;

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, userId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, userId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateDeleters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Deleters = new DeleterPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Deleters = new Deleter[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateDeleters_Ok_Data))]
    public async void UpdateDeleters_Ok(string authorId, string userId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

        List<Deleter> mappedDeleters = new() { };
        for (int i = 0; i < dto.Deleters!.Length; i++)
        {
            Deleter mappedDeleter = new Deleter() { };
            Fixture.IMapper.Setup<Deleter>(o => o.Map<Deleter>(dto.Deleters[i])).Returns(mappedDeleter);
            mappedDeleters.Add(mappedDeleter);

        }
        user.UserPermissions.Deleters = mappedDeleters.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateDeleters(authorId, userId, dto);
    }

    public static IEnumerable<object?[]> UpdateDeleters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                "id",
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                "id",
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Deleters = new DeleterPatchDto[] { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Deleters = new DeleterPatchDto[] { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { Deleters = new DeleterPatchDto[] { } },
                new User() { UserPermissions = new UserPermissions() { Deleters = new Deleter[] { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateDeleters_NotOk_Data))]
    public async void UpdateDeleters_NotOk(string authorId, string userId, UserPrivilegesPatchDto dto, User? user)
    {
        if (userId == "id" || authorId == "id")
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, userId, dto));
            if (userId == "id") Assert.Equal("userId", ex.Message);
            else Assert.Equal("authorId", ex.Message);
        }
        else if (dto.Deleters == null)
        {
            Exception ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateDeleters(authorId, userId, dto));
            Assert.Equal("dto", ex.Message);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateDeleters(authorId, userId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateDeleters(authorId, userId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(userId))).Returns(Task.FromResult<User?>(user));

            List<Deleter> mappedDeleters = new() { };
            for (int i = 0; i < dto.Deleters!.Length; i++)
            {
                Deleter mappedDeleter = new Deleter() { };
                Fixture.IMapper.Setup<Deleter>(o => o.Map<Deleter>(dto.Deleters[i])).Returns(mappedDeleter);
                mappedDeleters.Add(mappedDeleter);

            }
            user!.UserPermissions.Deleters = mappedDeleters.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateDeleters(authorId, userId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(userId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateDeleters(authorId, userId, dto));
        }
    }
}
