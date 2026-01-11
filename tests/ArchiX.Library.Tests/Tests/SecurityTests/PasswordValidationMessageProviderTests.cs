using ArchiX.Library.Runtime.Security;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests;

public class PasswordValidationMessageProviderTests
{
    private readonly PasswordValidationMessageProvider _provider;

    public PasswordValidationMessageProviderTests()
    {
        _provider = new PasswordValidationMessageProvider(NullLogger<PasswordValidationMessageProvider>.Instance);
    }

    [Fact]
    public void GetMessage_TurkishCulture_ReturnsLocalizedMessage()
    {
        _provider.SetCulture("tr-TR");

        var message = _provider.GetMessage("EMPTY");

        Assert.Equal("Parola boş olamaz.", message);
    }

    [Fact]
    public void GetMessage_EnglishCulture_ReturnsLocalizedMessage()
    {
        _provider.SetCulture("en-US");

        var message = _provider.GetMessage("EMPTY");

        Assert.Equal("Password cannot be empty.", message);
    }

    [Fact]
    public void GetMessage_WithParameters_FormatsCorrectly()
    {
        _provider.SetCulture("tr-TR");

        var message = _provider.GetMessage("MIN_LENGTH", 12);

        Assert.Equal("Parola en az 12 karakter olmalıdır.", message);
    }

    [Fact]
    public void GetMessage_WithParametersEnglish_FormatsCorrectly()
    {
        _provider.SetCulture("en-US");

        var message = _provider.GetMessage("MIN_LENGTH", 12);

        Assert.Equal("Password must be at least 12 characters long.", message);
    }

    [Fact]
    public void GetMessage_InvalidErrorCode_ReturnsErrorCode()
    {
        _provider.SetCulture("tr-TR");

        var message = _provider.GetMessage("INVALID_CODE");

        Assert.Equal("INVALID_CODE", message);
    }

    [Fact]
    public void GetMessage_EmptyErrorCode_ReturnsEmptyString()
    {
        var message = _provider.GetMessage("");

        Assert.Equal("", message);
    }

    [Fact]
    public void GetMessage_NullErrorCode_ReturnsEmptyString()
    {
        var message = _provider.GetMessage(null!);

        Assert.Equal("", message);
    }

    [Fact]
    public void GetMessages_MultipleErrorCodes_ReturnsAllMessages()
    {
        _provider.SetCulture("tr-TR");
        var errorCodes = new[] { "EMPTY", "REQ_UPPER", "REQ_DIGIT" };

        var messages = _provider.GetMessages(errorCodes);

        Assert.Equal(3, messages.Count);
        Assert.Contains("Parola boş olamaz.", messages);
        Assert.Contains("Parola en az bir büyük harf içermelidir.", messages);
        Assert.Contains("Parola en az bir rakam içermelidir.", messages);
    }

    [Fact]
    public void GetMessages_EmptyList_ReturnsEmptyList()
    {
        var messages = _provider.GetMessages([]);

        Assert.Empty(messages);
    }

    [Fact]
    public void SetCulture_InvalidCulture_UsesDefault()
    {
        _provider.SetCulture("invalid-culture");

        var message = _provider.GetMessage("EMPTY");

        Assert.NotEmpty(message);
    }

    [Fact]
    public void GetMessage_AllErrorCodes_Turkish_ReturnsCorrectMessages()
    {
        _provider.SetCulture("tr-TR");

        Assert.Equal("Parola boş olamaz.", _provider.GetMessage("EMPTY"));
        Assert.Equal("Bu parola yasaklı kelimeler listesinde bulunuyor.", _provider.GetMessage("BLOCK_LIST"));
        Assert.Equal("Bu parola yasaklanmış kelimeler içeriyor.", _provider.GetMessage("BLACKLIST"));
        Assert.Equal("Bu parola dinamik engelleme listesinde bulunuyor.", _provider.GetMessage("DYNAMIC_BLOCK"));
        Assert.Equal("Parolanızın süresi dolmuştur. Lütfen yeni bir parola belirleyin.", _provider.GetMessage("EXPIRED"));
        Assert.Equal("Bu parola daha önce veri sızıntılarında tespit edilmiş. Lütfen farklı bir parola seçin.", _provider.GetMessage("PWNED"));
        Assert.Equal("Bu parolayı daha önce kullandınız. Lütfen farklı bir parola seçin.", _provider.GetMessage("HISTORY"));
        Assert.Equal("Bu parola çok yaygın kullanılan bir kelime. Lütfen daha güvenli bir parola seçin.", _provider.GetMessage("DICTIONARY_WORD"));
        Assert.Equal("Parola yeterince karmaşık değil. Lütfen daha güçlü bir parola seçin.", _provider.GetMessage("LOW_ENTROPY"));
    }

    [Fact]
    public void GetMessage_AllErrorCodes_English_ReturnsCorrectMessages()
    {
        _provider.SetCulture("en-US");

        Assert.Equal("Password cannot be empty.", _provider.GetMessage("EMPTY"));
        Assert.Equal("This password is in the blocked words list.", _provider.GetMessage("BLOCK_LIST"));
        Assert.Equal("This password contains blacklisted words.", _provider.GetMessage("BLACKLIST"));
        Assert.Equal("This password is in the dynamic block list.", _provider.GetMessage("DYNAMIC_BLOCK"));
        Assert.Equal("Your password has expired. Please set a new password.", _provider.GetMessage("EXPIRED"));
        Assert.Equal("This password has been found in data breaches. Please choose a different password.", _provider.GetMessage("PWNED"));
        Assert.Equal("You have used this password before. Please choose a different password.", _provider.GetMessage("HISTORY"));
        Assert.Equal("This password is a commonly used word. Please choose a more secure password.", _provider.GetMessage("DICTIONARY_WORD"));
        Assert.Equal("Password is not complex enough. Please choose a stronger password.", _provider.GetMessage("LOW_ENTROPY"));
    }
}
