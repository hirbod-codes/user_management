namespace user_management.Dtos.Client;

public class ClientRetrieveDto
{
    public string? Id { get; set; }
    public string? Secret { get; set; }
    public string? RedirectUrl { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}