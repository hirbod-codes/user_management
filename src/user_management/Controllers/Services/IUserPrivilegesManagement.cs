using user_management.Dtos.User;

namespace user_management.Controllers.Services;

public interface IUserPrivilegesManagement
{
    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task UpdateReaders(string authorId, string userId, UserPrivilegesPatchDto dto);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task UpdateAllReaders(string authorId, string userId, UserPrivilegesPatchDto dto);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task UpdateUpdaters(string authorId, string userId, UserPrivilegesPatchDto dto);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task UpdateAllUpdaters(string authorId, string userId, UserPrivilegesPatchDto dto);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task UpdateDeleters(string authorId, string userId, UserPrivilegesPatchDto dto);
}