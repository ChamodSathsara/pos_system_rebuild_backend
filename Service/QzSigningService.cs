using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PosApi.Configuration;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class QzSigningService : IQzSigningService
{
    private readonly QzSigningSettings _settings;
    private readonly IHostEnvironment _environment;

    public QzSigningService(
        IOptions<QzSigningSettings> settings,
        IHostEnvironment environment)
    {
        _settings = settings.Value;
        _environment = environment;
    }

    public async Task<string> GetCertificateAsync(CancellationToken cancellationToken = default)
    {
        var path = GetRequiredPath(_settings.CertificatePath, "QZ certificate");
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task<string> SignAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        var path = GetRequiredPath(_settings.PrivateKeyPath, "QZ private key");
        var privateKeyPem = await File.ReadAllTextAsync(path, cancellationToken);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(
            payload.Span,
            HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }

    private string GetRequiredPath(string configuredPath, string description)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException($"{description} path is not configured.");
        }

        var fullPath = Path.IsPathRooted(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(configuredPath, _environment.ContentRootPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"{description} file was not found.");
        }

        return fullPath;
    }
}
