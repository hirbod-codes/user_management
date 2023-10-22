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

    private UserPrivilegesManagement InstantiateService() => new(Fixture.IUserRepository.Object, Fixture.IMapper.Object);

    public static Faker Faker = new("en");

    public static IEnumerable<object?[]> UpdateReaders_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Readers = Array.Empty<ReaderPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Readers = Array.Empty<Reader>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateReaders_Ok_Data))]
    public async void UpdateReaders_Ok(string authorId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

        List<Reader> mappedReaders = new() { };
        for (int i = 0; i < dto.Readers!.Length; i++)
        {
            Reader mappedReader = new Reader() { };
            Fixture.IMapper.Setup<Reader>(o => o.Map<Reader>(dto.Readers[i])).Returns(mappedReader);
            mappedReaders.Add(mappedReader);

        }
        user.UserPermissions.Readers = mappedReaders.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateReaders(authorId, dto);
    }

    public static IEnumerable<object?[]> UpdateReaders_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                "id",
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Readers = Array.Empty<ReaderPatchDto>() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Readers = Array.Empty<ReaderPatchDto>() },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Readers = Array.Empty<ReaderPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Readers = Array.Empty<Reader>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateReaders_NotOk_Data))]
    public async void UpdateReaders_NotOk(string authorId, UserPrivilegesPatchDto dto, User? user)
    {
        if (dto.UserId == "id" || authorId == "id")
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateReaders(authorId, dto));
            if (dto.UserId == "id") Assert.Equal("dto", ex.ParamName);
            else Assert.Equal("authorId", ex.ParamName);
        }
        else if (dto.Readers == null)
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateReaders(authorId, dto));
            Assert.Equal("dto", ex.ParamName);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateReaders(authorId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateReaders(authorId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

            List<Reader> mappedReaders = new() { };
            for (int i = 0; i < dto.Readers!.Length; i++)
            {
                Reader mappedReader = new Reader() { };
                Fixture.IMapper.Setup<Reader>(o => o.Map<Reader>(dto.Readers[i])).Returns(mappedReader);
                mappedReaders.Add(mappedReader);

            }
            user!.UserPermissions.Readers = mappedReaders.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateReaders(authorId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateReaders(authorId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateAllReaders_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllReaders = new AllReaders() { } },
                new User() { UserPermissions = new UserPermissions() { AllReaders = new AllReaders() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllReaders_Ok_Data))]
    public async void UpdateAllReaders_Ok(string authorId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

        AllReaders mappedAllReaders = new() { };
        Fixture.IMapper.Setup<AllReaders>(o => o.Map<AllReaders>(dto.AllReaders)).Returns(mappedAllReaders);
        user.UserPermissions.AllReaders = mappedAllReaders;

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateAllReaders(authorId, dto);
    }

    public static IEnumerable<object?[]> UpdateAllReaders_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                "id",
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllReaders = new AllReaders() { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllReaders = new AllReaders() { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllReaders = new AllReaders() { } },
                new User() { UserPermissions = new UserPermissions() { AllReaders = new AllReaders() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllReaders_NotOk_Data))]
    public async void UpdateAllReaders_NotOk(string authorId, UserPrivilegesPatchDto dto, User? user)
    {
        if (dto.UserId == "id" || authorId == "id")
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
            if (dto.UserId == "id") Assert.Equal("dto", ex.ParamName);
            else Assert.Equal("authorId", ex.ParamName);
        }
        else if (dto.AllReaders == null)
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
            Assert.Equal("dto", ex.ParamName);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

            AllReaders mappedAllReaders = new() { };
            Fixture.IMapper.Setup<AllReaders>(o => o.Map<AllReaders>(dto.AllReaders)).Returns(mappedAllReaders);
            user!.UserPermissions.AllReaders = mappedAllReaders;

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateUpdaters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Updaters = Array.Empty<UpdaterPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Updaters = Array.Empty<Updater>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateUpdaters_Ok_Data))]
    public async void UpdateUpdaters_Ok(string authorId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

        List<Updater> mappedUpdaters = new() { };
        for (int i = 0; i < dto.Updaters!.Length; i++)
        {
            Updater mappedUpdater = new Updater() { };
            Fixture.IMapper.Setup<Updater>(o => o.Map<Updater>(dto.Updaters[i])).Returns(mappedUpdater);
            mappedUpdaters.Add(mappedUpdater);

        }
        user.UserPermissions.Updaters = mappedUpdaters.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateUpdaters(authorId, dto);
    }

    public static IEnumerable<object?[]> UpdateUpdaters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId ="id" },
                null
            },
            new object?[] {
                "id",
                new UserPrivilegesPatchDto() { UserId ="id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Updaters = Array.Empty<UpdaterPatchDto>() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Updaters = Array.Empty<UpdaterPatchDto>() },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Updaters = Array.Empty<UpdaterPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Updaters = Array.Empty<Updater>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateUpdaters_NotOk_Data))]
    public async void UpdateUpdaters_NotOk(string authorId, UserPrivilegesPatchDto dto, User? user)
    {
        if (dto.UserId == "id" || authorId == "id")
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
            if (dto.UserId == "id") Assert.Equal("dto", ex.ParamName);
            else Assert.Equal("authorId", ex.ParamName);
        }
        else if (dto.Updaters == null)
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateUpdaters(authorId, dto));
            Assert.Equal("dto", ex.ParamName);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateUpdaters(authorId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateUpdaters(authorId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

            List<Updater> mappedUpdaters = new() { };
            for (int i = 0; i < dto.Updaters!.Length; i++)
            {
                Updater mappedUpdater = new Updater() { };
                Fixture.IMapper.Setup<Updater>(o => o.Map<Updater>(dto.Updaters[i])).Returns(mappedUpdater);
                mappedUpdaters.Add(mappedUpdater);

            }
            user!.UserPermissions.Updaters = mappedUpdaters.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateUpdaters(authorId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateUpdaters(authorId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateAllUpdaters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllUpdaters = new AllUpdaters() { } },
                new User() { UserPermissions = new UserPermissions() { AllUpdaters = new AllUpdaters() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllUpdaters_Ok_Data))]
    public async void UpdateAllUpdaters_Ok(string authorId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

        AllUpdaters mappedAllReaders = new() { };
        Fixture.IMapper.Setup<AllUpdaters>(o => o.Map<AllUpdaters>(dto.AllUpdaters)).Returns(mappedAllReaders);
        user.UserPermissions.AllUpdaters = mappedAllReaders;

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateAllUpdaters(authorId, dto);
    }

    public static IEnumerable<object?[]> UpdateAllUpdaters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                "id",
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString()},
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllUpdaters = new AllUpdaters() { } },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllUpdaters = new AllUpdaters() { } },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), AllUpdaters = new AllUpdaters() { } },
                new User() { UserPermissions = new UserPermissions() { AllUpdaters = new AllUpdaters() { } } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateAllUpdaters_NotOk_Data))]
    public async void UpdateAllUpdaters_NotOk(string authorId, UserPrivilegesPatchDto dto, User? user)
    {
        if (dto.UserId == "id" || authorId == "id")
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
            if (dto.UserId == "id") Assert.Equal("dto", ex.ParamName);
            else Assert.Equal("authorId", ex.ParamName);
        }
        else if (dto.AllUpdaters == null)
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, dto));
            Assert.Equal("dto", ex.ParamName);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

            AllUpdaters mappedAllReaders = new() { };
            Fixture.IMapper.Setup<AllUpdaters>(o => o.Map<AllUpdaters>(dto.AllUpdaters)).Returns(mappedAllReaders);
            user!.UserPermissions.AllUpdaters = mappedAllReaders;

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateAllUpdaters(authorId, dto));
        }
    }

    public static IEnumerable<object?[]> UpdateDeleters_Ok_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Deleters = Array.Empty<DeleterPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Deleters = Array.Empty<Deleter>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateDeleters_Ok_Data))]
    public async void UpdateDeleters_Ok(string authorId, UserPrivilegesPatchDto dto, User user)
    {
        Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

        List<Deleter> mappedDeleters = new() { };
        for (int i = 0; i < dto.Deleters!.Length; i++)
        {
            Deleter mappedDeleter = new Deleter() { };
            Fixture.IMapper.Setup<Deleter>(o => o.Map<Deleter>(dto.Deleters[i])).Returns(mappedDeleter);
            mappedDeleters.Add(mappedDeleter);

        }
        user.UserPermissions.Deleters = mappedDeleters.ToArray();

        Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user.UserPermissions)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().UpdateDeleters(authorId, dto);
    }

    public static IEnumerable<object?[]> UpdateDeleters_NotOk_Data =>
        new List<object?[]>
        {
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                "id",
                new UserPrivilegesPatchDto() { UserId = "id" },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Deleters = Array.Empty<DeleterPatchDto>() },
                null
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Deleters = Array.Empty<DeleterPatchDto>() },
                new User() { }
            },
            new object?[] {
                ObjectId.GenerateNewId().ToString(),
                new UserPrivilegesPatchDto() { UserId = ObjectId.GenerateNewId().ToString(), Deleters = Array.Empty<DeleterPatchDto>() },
                new User() { UserPermissions = new UserPermissions() { Deleters = Array.Empty<Deleter>() } }
            }
        };

    [Theory]
    [MemberData(nameof(UpdateDeleters_NotOk_Data))]
    public async void UpdateDeleters_NotOk(string authorId, UserPrivilegesPatchDto dto, User? user)
    {
        if (dto.UserId == "id" || authorId == "id")
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateAllReaders(authorId, dto));
            if (dto.UserId == "id") Assert.Equal("dto", ex.ParamName);
            else Assert.Equal("authorId", ex.ParamName);
        }
        else if (dto.Deleters == null)
        {
            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateDeleters(authorId, dto));
            Assert.Equal("dto", ex.ParamName);
        }
        else if (user == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateDeleters(authorId, dto));
        }
        else if (user != null && user.UserPermissions == null)
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateDeleters(authorId, dto));
        }
        else
        {
            Fixture.IUserRepository.Setup<Task<User?>>(o => o.RetrieveById(ObjectId.Parse(dto.UserId))).Returns(Task.FromResult<User?>(user));

            List<Deleter> mappedDeleters = new() { };
            for (int i = 0; i < dto.Deleters!.Length; i++)
            {
                Deleter mappedDeleter = new Deleter() { };
                Fixture.IMapper.Setup<Deleter>(o => o.Map<Deleter>(dto.Deleters[i])).Returns(mappedDeleter);
                mappedDeleters.Add(mappedDeleter);

            }
            user!.UserPermissions.Deleters = mappedDeleters.ToArray();

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(null));
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateDeleters(authorId, dto));

            Fixture.IUserRepository.Setup<Task<bool?>>(o => o.UpdateUserPrivileges(ObjectId.Parse(authorId), ObjectId.Parse(dto.UserId), user!.UserPermissions)).Returns(Task.FromResult<bool?>(false));
            await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateDeleters(authorId, dto));
        }
    }
}
