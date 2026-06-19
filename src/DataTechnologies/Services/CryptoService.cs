using System.Security.Cryptography;
using System.Text;

namespace DataTechnologies.Services;

// Reversible field-level encryption (AES-256-GCM) plus a deterministic HMAC
// "blind index" for equality lookups on otherwise-unreadable columns.
public sealed class CryptoService : IFieldCipher
{
    private const string Prefix = "v1:";
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _encryptionKey;
    private readonly byte[] _indexKey;

    public CryptoService(byte[] encryptionKey, byte[] indexKey)
    {
        ArgumentNullException.ThrowIfNull(encryptionKey);
        ArgumentNullException.ThrowIfNull(indexKey);
        if (encryptionKey.Length != KeySize)
            throw new ArgumentException($"Encryption key must be {KeySize} bytes.", nameof(encryptionKey));
        if (indexKey.Length != KeySize)
            throw new ArgumentException($"Index key must be {KeySize} bytes.", nameof(indexKey));

        _encryptionKey = (byte[])encryptionKey.Clone();
        _indexKey = (byte[])indexKey.Clone();
    }

    public static CryptoService FromEnvironment()
    {
        return new CryptoService(ReadKey("PII_ENCRYPTION_KEY"), ReadKey("PII_INDEX_KEY"));
    }

    public string Encrypt(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        using (var aes = new AesGcm(_encryptionKey, TagSize))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        byte[] combined = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, combined, NonceSize + TagSize, cipherBytes.Length);

        return Prefix + Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (!ciphertext.StartsWith(Prefix, StringComparison.Ordinal))
            throw new FormatException("Value is not encrypted with the expected scheme.");

        byte[] combined = Convert.FromBase64String(ciphertext[Prefix.Length..]);
        if (combined.Length < NonceSize + TagSize)
            throw new FormatException("Encrypted value is malformed.");

        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] cipherBytes = new byte[combined.Length - NonceSize - TagSize];
        Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(combined, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(combined, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

        byte[] plainBytes = new byte[cipherBytes.Length];
        using (var aes = new AesGcm(_encryptionKey, TagSize))
        {
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string BlindIndex(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string normalized = value.Trim().ToUpperInvariant();
        byte[] hash = HMACSHA256.HashData(_indexKey, Encoding.UTF8.GetBytes(normalized));
        return Convert.ToBase64String(hash);
    }

    private static byte[] ReadKey(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"{variableName} is not set. Add a 32-byte Base64 key to your .env file.");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(value);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException($"{variableName} is not valid Base64.", ex);
        }

        if (key.Length != KeySize)
            throw new InvalidOperationException($"{variableName} must decode to {KeySize} bytes.");

        return key;
    }
}
