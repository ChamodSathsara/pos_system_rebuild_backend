namespace PosApi.Service.Interfaces;

public interface IQzSigningService
{
    Task<string> GetCertificateAsync(CancellationToken cancellationToken = default);
    Task<string> SignAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}
