using AutoMapper;
using user_management.Controllers.Services;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services.Data;
using user_management.Services.Data.User;

namespace user_management.Services;

public class UserPrivilegesManagement : IUserPrivilegesManagement
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserPrivilegesManagement(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task UpdateReaders(string authorId, UserPrivilegesPatchDto dto)
    {
        if (dto.Readers == null) throw new ArgumentException(null, nameof(dto));

        User user = await _userRepository.RetrieveById(dto.UserId) ?? throw new DataNotFoundException();
        if (user.UserPermissions == null) throw new OperationException();

        user.UserPermissions.Readers = dto.Readers.ToList().ConvertAll<Reader>(rpd => _mapper.Map<Reader>(rpd)).ToArray();

        bool r = await _userRepository.UpdateUserPrivileges(authorId, dto.UserId, user.UserPermissions) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateAllReaders(string authorId, UserPrivilegesPatchDto dto)
    {
        if (dto.AllReaders == null) throw new ArgumentException(null, nameof(dto));

        User user = await _userRepository.RetrieveById(dto.UserId) ?? throw new DataNotFoundException();
        if (user.UserPermissions == null) throw new OperationException();

        user.UserPermissions.AllReaders = _mapper.Map<AllReaders>(dto.AllReaders);

        bool r = await _userRepository.UpdateUserPrivileges(authorId, dto.UserId, user.UserPermissions) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateUpdaters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (dto.Updaters == null) throw new ArgumentException(null, nameof(dto));

        User user = await _userRepository.RetrieveById(dto.UserId) ?? throw new DataNotFoundException();
        if (user.UserPermissions == null) throw new OperationException();

        user.UserPermissions.Updaters = dto.Updaters.ToList().ConvertAll<Updater>(rpd => _mapper.Map<Updater>(rpd)).ToArray();

        bool r = await _userRepository.UpdateUserPrivileges(authorId, dto.UserId, user.UserPermissions) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateAllUpdaters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (dto.AllUpdaters == null) throw new ArgumentException(null, nameof(dto));

        User user = await _userRepository.RetrieveById(dto.UserId) ?? throw new DataNotFoundException();
        if (user.UserPermissions == null) throw new OperationException();

        user.UserPermissions.AllUpdaters = _mapper.Map<AllUpdaters>(dto.AllUpdaters);

        bool r = await _userRepository.UpdateUserPrivileges(authorId, dto.UserId, user.UserPermissions) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateDeleters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (dto.Deleters == null) throw new ArgumentException(null, nameof(dto));

        User user = await _userRepository.RetrieveById(dto.UserId) ?? throw new DataNotFoundException();
        if (user.UserPermissions == null) throw new OperationException();

        user.UserPermissions.Deleters = dto.Deleters.ToList().ConvertAll<Deleter>(rpd => _mapper.Map<Deleter>(rpd)).ToArray();

        bool r = await _userRepository.UpdateUserPrivileges(authorId, dto.UserId, user.UserPermissions) ?? throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }
}
