using FluentAssertions;
using Vargshala.Infrastructure.Services;
using Xunit.Abstractions;

namespace Vargshala.UnitTests.Services;

public class EncryptionServiceTests
{
    private readonly ITestOutputHelper _output;
    private const string MasterKey = "aU5FU1RIQY5NUzU3Q1JFVEtFWTk4NzY1NDMyMUFCQ0RFRkdISUdLTE1OTw==";

    public EncryptionServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Encrypt_Admin123_PrintsEncryptedTextAndDecryptsSuccessfully()
    {
        var service = new EncryptionService();
        var plainText = "Admin@123";

        var encrypted = service.Encrypt(plainText, MasterKey);
        _output.WriteLine($"ENCRYPTED_OUTPUT: {encrypted}");

        encrypted.Should().NotBeNullOrWhiteSpace();

        var decrypted = service.Decrypt(encrypted, MasterKey);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_And_Decrypt_ReturnsOriginalText()
    {
        var service = new EncryptionService();
        var original = "Admin@123";

        var encrypted = service.Encrypt(original, MasterKey);
        var decrypted = service.Decrypt(encrypted, MasterKey);

        decrypted.Should().Be(original);
    }
}
