using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PosApi.Configuration;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class QzSigningService : IQzSigningService
{
    private readonly QzSigningSettings _settings;

    public QzSigningService(IOptions<QzSigningSettings> settings)
    {
        _settings = settings.Value;
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

    private static string GetRequiredPath(string configuredPath, string description)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException($"{description} path is not configured.");
        }

        var fullPath = Path.GetFullPath(configuredPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"{description} file was not found.");
        }

        return fullPath;
    }
}
