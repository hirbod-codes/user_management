namespace user_management.Dtos.User;

using user_management.Models;

public class UserPrivilegesRetrieveDto
{
    public ReaderRetrieveDto[] Readers { get; set; } = new ReaderRetrieveDto[] { };
    public AllReaders? AllReaders { get; set; }
    public UpdaterRetrieveDto[] Updaters { get; set; } = new UpdaterRetrieveDto[] { };
    public AllUpdaters? AllUpdaters { get; set; }
    public DeleterRetrieveDto[] Deleters { get; set; } = new DeleterRetrieveDto[] { };
}