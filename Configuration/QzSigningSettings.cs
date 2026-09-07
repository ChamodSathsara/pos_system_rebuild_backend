namespace PosApi.Configuration;

public class QzSigningSettings
{
    public const string SectionName = "QzSigning";

    public string CertificatePath { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public int MaxPayloadBytes { get; set; } = 100 * 1024;
}
