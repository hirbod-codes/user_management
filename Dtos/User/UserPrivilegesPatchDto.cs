namespace user_management.Dtos.User;

using user_management.Models;

public class UserPrivilegesPatchDto
{
    public string Id { get; set; } = null!;
    public ReaderPatchDto[] Readers { get; set; } = new ReaderPatchDto[] { };
    public AllReaders? AllReaders { get; set; }
    public UpdaterPatchDto[] Updaters { get; set; } = new UpdaterPatchDto[] { };
    public AllUpdaters? AllUpdaters { get; set; }
    public DeleterPatchDto[] Deleters { get; set; } = new DeleterPatchDto[] { };
}