using System.IO.Abstractions;
using user_management.Utilities;

namespace user_management.Configuration.Extensions.DockerSecrets;

public class DockerSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _secretsDirectoryPath;
    private readonly string _colonPlaceholder;
    private readonly ICollection<string> _allowedPrefixes;
    private readonly IFileSystem _fileSystem;

    public DockerSecretsConfigurationProvider(string secretsDirectoryPath, string colonPlaceholder, ICollection<string> allowedPrefixes) : this(secretsDirectoryPath, colonPlaceholder, allowedPrefixes, new FileSystem())
    { }

    public DockerSecretsConfigurationProvider(string secretsDirectoryPath, string colonPlaceholder, ICollection<string> allowedPrefixes, IFileSystem fileSystem)
    {
        _secretsDirectoryPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath));
        _colonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder));
        _allowedPrefixes = allowedPrefixes;
        _fileSystem = fileSystem;
    }

    public override void Load()
    {
        if (!_fileSystem.Directory.Exists(_secretsDirectoryPath)) return;

        foreach (string secretFilePath in _fileSystem.Directory.EnumerateFiles(_secretsDirectoryPath))
            ProcessFile(secretFilePath);
    }

    private void ProcessFile(string secretFilePath)
    {
        if (string.IsNullOrWhiteSpace(secretFilePath) || !_fileSystem.File.Exists(secretFilePath)) return;

        string secretFileName = _fileSystem.Path.GetFileName(secretFilePath);

        if (string.IsNullOrWhiteSpace(secretFileName)) return;

        string? thisFilePrefix = null;
        if (
            _allowedPrefixes != null
            && _allowedPrefixes.Count > 0
            && !_allowedPrefixes.Any(prefix =>
            {
                bool result = secretFileName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
                if (result) thisFilePrefix = prefix;
                return result;
            })
        )
            return;

        using (var reader = new StreamReader(_fileSystem.File.OpenRead(secretFilePath)))
        {
            string secretValue = reader.ReadToEnd();
            if (secretValue.EndsWith(Environment.NewLine))
                secretValue = secretValue.Substring(0, secretValue.Length - 1);

            string secretKey = secretFileName.Replace(_colonPlaceholder, ":");
            Data.Add(secretKey, secretValue);

            if (thisFilePrefix != null)
            {
                secretKey = secretKey.TrimStart(thisFilePrefix, StringComparison.InvariantCultureIgnoreCase);
                Data.Add(secretKey, secretValue);
            }
        }
    }
}
