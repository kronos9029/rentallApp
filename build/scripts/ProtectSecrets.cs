using System.Security.Cryptography;
using System.Text;

const string EncryptedValuePrefix = "enc::";

var arguments = ParseArguments(args);
if (!arguments.TryGetValue("key-base64", out var keyBase64))
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  dotnet run ProtectSecrets.cs -- --value <secret> --key-base64 <base64>");
    Console.Error.WriteLine("  dotnet run ProtectSecrets.cs -- --empty-value --key-base64 <base64>");
    Console.Error.WriteLine("  dotnet run ProtectSecrets.cs -- --plaintext <path> --output <path> --key-base64 <base64>");
    return 1;
}

var key = Convert.FromBase64String(keyBase64);
if (key.Length is not 16 and not 24 and not 32)
{
    Console.Error.WriteLine("AES key must be 128, 192, or 256 bits.");
    return 1;
}

if (arguments.TryGetValue("value", out var value))
{
    Console.WriteLine(EncryptValue(value, keyBase64));
    return 0;
}

if (arguments.ContainsKey("empty-value"))
{
    Console.WriteLine(EncryptValue(string.Empty, keyBase64));
    return 0;
}

if (!arguments.TryGetValue("plaintext", out var plaintextPath) ||
    !arguments.TryGetValue("output", out var outputPath))
{
    Console.Error.WriteLine("Both --plaintext and --output are required for file encryption mode.");
    return 1;
}

var plaintext = await File.ReadAllTextAsync(plaintextPath, Encoding.UTF8);
var nonce = RandomNumberGenerator.GetBytes(12);
var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
var ciphertext = new byte[plaintextBytes.Length];
var tag = new byte[16];

using (var aes = new AesGcm(key, 16))
{
    aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
}

var payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
Buffer.BlockCopy(ciphertext, 0, payload, nonce.Length + tag.Length, ciphertext.Length);

var directory = Path.GetDirectoryName(outputPath);
if (!string.IsNullOrWhiteSpace(directory))
{
    Directory.CreateDirectory(directory);
}

await File.WriteAllBytesAsync(outputPath, payload);
return 0;

static string EncryptValue(string plaintext, string keyBase64)
{
    var key = Convert.FromBase64String(keyBase64);
    var nonce = RandomNumberGenerator.GetBytes(12);
    var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
    var ciphertext = new byte[plaintextBytes.Length];
    var tag = new byte[16];

    using (var aes = new AesGcm(key, 16))
    {
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
    }

    var payload = new byte[nonce.Length + tag.Length + ciphertext.Length];
    Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
    Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
    Buffer.BlockCopy(ciphertext, 0, payload, nonce.Length + tag.Length, ciphertext.Length);
    return $"{EncryptedValuePrefix}{Convert.ToBase64String(payload)}";
}

static Dictionary<string, string> ParseArguments(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var index = 0; index < args.Length; index++)
    {
        var current = args[index];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = current[2..];
        if (index + 1 >= args.Length)
        {
            result[key] = string.Empty;
            break;
        }

        if (args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            result[key] = string.Empty;
            continue;
        }

        result[key] = args[index + 1];
        index++;
    }

    return result;
}
