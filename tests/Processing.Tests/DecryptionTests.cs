using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RbcTestTask.Api.Services;
using TestTask.Models;
using Xunit;

public class DecryptionTests
{
    private static RequestModel Request(byte[] key, byte[] ciphertext) => new()
    {
        UrlB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("https://example.invalid")),
        PageB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("<p>synthetic</p>")),
        Selector = "missing-element", Attribute = "id",
        KeyBytesB64 = Convert.ToBase64String(key), EncryptedTextBytesB64 = Convert.ToBase64String(ciphertext)
    };

    [Theory]
    [InlineData(16, 16)]
    [InlineData(32, 1)]
    public async Task Invalid_encryption_inputs_are_errors_before_database_access(int keyLength, int ciphertextLength)
    {
        var service = new ProcessingService(new ConfigurationBuilder().Build());
        var response = await service.ProcessAsync(Request(new byte[keyLength], new byte[ciphertextLength]));
        Assert.Equal(1, response.IsError);
        Assert.Equal("DecryptionError", response.ErrorCode);
    }

    [Fact]
    public async Task Valid_AES256_input_returns_the_plaintext()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = new byte[16]; Encoding.UTF8.GetBytes("demo").CopyTo(plaintext, 0);
        using var aes = Aes.Create(); aes.Key = key; aes.Mode = CipherMode.ECB; aes.Padding = PaddingMode.None;
        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
        var response = await new ProcessingService(new ConfigurationBuilder().Build()).ProcessAsync(Request(key, ciphertext));
        Assert.Equal(0, response.IsError);
        Assert.Equal("demo", response.DecryptedPlainText);
    }
}
