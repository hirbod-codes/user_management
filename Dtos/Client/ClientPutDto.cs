namespace user_management.Dtos.Client;

public class ClientPutDto
{
    public string Id { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
}