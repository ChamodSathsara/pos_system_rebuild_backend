namespace PosApi.Security;

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}

/// <summary>
/// BCrypt-based password hasher. BCrypt embeds its own salt in the resulting hash, so no
/// separate salt column is required on system_user.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));
        }
        Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor));

        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
    }

    public bool Verify(string plainTextPassword, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash was not produced by BCrypt (e.g. legacy data) - treat as non-match rather than throwing.
            return false;
        }
    }
}
