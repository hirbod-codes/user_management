namespace user_management.Dtos.User;

using user_management.Models;

public class UserPrivilegesPatchDto
{
    public ReaderPatchDto[]? Readers { get; set; } = null;
    public AllReaders? AllReaders { get; set; } = null;
    public UpdaterPatchDto[]? Updaters { get; set; } = null;
    public AllUpdaters? AllUpdaters { get; set; } = null;
    public DeleterPatchDto[]? Deleters { get; set; } = null;
}