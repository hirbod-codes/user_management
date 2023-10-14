using user_management.Configuration.Extensions.DockerSecrets;

namespace user_management.Configuration.Sources.DockerSecrets;

public class DockerSecretsConfigurationsSource : IConfigurationSource
{
    private readonly string _secretsDirectoryPath;
    private readonly string _colonPlaceholder;
    private readonly ICollection<string> _allowedPrefixes;

    public DockerSecretsConfigurationsSource(string secretsDirectoryPath, string colonPlaceholder, ICollection<string>? allowedPrefixes = null)
    {
        _secretsDirectoryPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath));
        _colonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder));
        _allowedPrefixes = allowedPrefixes ?? Array.Empty<string>();
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new DockerSecretsConfigurationProvider(_secretsDirectoryPath, _colonPlaceholder, _allowedPrefixes);
}
