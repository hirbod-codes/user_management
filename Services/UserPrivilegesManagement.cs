using AutoMapper;
using MongoDB.Bson;
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
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId)) throw new ArgumentException("authorId");

        if (dto.Readers == null) throw new ArgumentException("dto");

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null) throw new DataNotFoundException();
        if (author.UserPrivileges == null) throw new OperationException();

        author.UserPrivileges.Readers = dto.Readers.ToList().ConvertAll<Reader>(rpd => _mapper.Map<Reader>(rpd)).ToArray();

        bool? r = await _userRepository.UpdateUserPrivileges(author);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateAllReaders(string authorId, UserPrivilegesPatchDto dto)
    {
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))  throw new ArgumentException("authorId");

        if (dto.AllReaders == null)  throw new ArgumentException("dto");

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null) throw new DataNotFoundException();
        if (author.UserPrivileges == null) throw new OperationException();

        author.UserPrivileges.AllReaders = _mapper.Map<AllReaders>(dto.AllReaders);

        bool? r = await _userRepository.UpdateUserPrivileges(author);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateUpdaters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))  throw new ArgumentException("authorId");

        if (dto.Updaters == null)  throw new ArgumentException("dto");

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null) throw new DataNotFoundException();
        if (author.UserPrivileges == null) throw new OperationException();

        author.UserPrivileges.Updaters = dto.Updaters.ToList().ConvertAll<Updater>(rpd => _mapper.Map<Updater>(rpd)).ToArray();

        bool? r = await _userRepository.UpdateUserPrivileges(author);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateAllUpdaters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))  throw new ArgumentException("authorId");

        if (dto.AllUpdaters == null)  throw new ArgumentException("dto");

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null) throw new DataNotFoundException();
        if (author.UserPrivileges == null) throw new OperationException();

        author.UserPrivileges.AllUpdaters = _mapper.Map<AllUpdaters>(dto.AllUpdaters);

        bool? r = await _userRepository.UpdateUserPrivileges(author);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }

    public async Task UpdateDeleters(string authorId, UserPrivilegesPatchDto dto)
    {
        if (!ObjectId.TryParse(authorId, out ObjectId authorObjectId))  throw new ArgumentException("authorId");

        if (dto.Deleters == null)  throw new ArgumentException("dto");

        User? author = await _userRepository.RetrieveById(authorObjectId);
        if (author == null) throw new DataNotFoundException();
        if (author.UserPrivileges == null) throw new OperationException();

        author.UserPrivileges.Deleters = dto.Deleters.ToList().ConvertAll<Deleter>(rpd => _mapper.Map<Deleter>(rpd)).ToArray();

        bool? r = await _userRepository.UpdateUserPrivileges(author);
        if (r == null) throw new DataNotFoundException();
        if (r == false) throw new OperationException();
    }
}