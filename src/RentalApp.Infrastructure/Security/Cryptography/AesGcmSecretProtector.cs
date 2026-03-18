using System.Security.Cryptography;
using System.Text;

namespace RentalApp.Infrastructure.Security.Cryptography;

public static class AesGcmSecretProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static byte[] Encrypt(string plaintext, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ValidateKey(key);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return nonce
            .Concat(tag)
            .Concat(ciphertext)
            .ToArray();
    }

    public static string Decrypt(byte[] payload, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ValidateKey(key);

        if (payload.Length < NonceSize + TagSize)
        {
            throw new InvalidOperationException("Encrypted payload is invalid.");
        }

        var nonce = payload[..NonceSize];
        var tag = payload[NonceSize..(NonceSize + TagSize)];
        var ciphertext = payload[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public static byte[] ParseBase64Key(string base64Key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Key);

        var key = Convert.FromBase64String(base64Key);
        ValidateKey(key);
        return key;
    }

    private static void ValidateKey(byte[] key)
    {
        if (key.Length is not (16 or 24 or 32))
        {
            throw new InvalidOperationException("AES key must be 128, 192, or 256 bits.");
        }
    }
}
