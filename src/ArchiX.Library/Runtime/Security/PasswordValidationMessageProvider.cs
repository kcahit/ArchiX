using System.Globalization;
using System.Resources;

using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Parola doğrulama hata mesajlarını yerelleştirme servisi implementasyonu.
/// </summary>
public class PasswordValidationMessageProvider : IPasswordValidationMessageProvider
{
    private readonly ILogger<PasswordValidationMessageProvider> _logger;
    private readonly ResourceManager _resourceManager;
    private CultureInfo _currentCulture;

    public PasswordValidationMessageProvider(ILogger<PasswordValidationMessageProvider> logger)
    {
        _logger = logger;
        _resourceManager = new ResourceManager(
            "ArchiX.Library.Resources.PasswordValidation",
            typeof(PasswordValidationMessageProvider).Assembly);
        _currentCulture = CultureInfo.CurrentUICulture;
    }

    public string GetMessage(string errorCode, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return errorCode ?? string.Empty;
        }

        try
        {
            var template = _resourceManager.GetString(errorCode, _currentCulture);

            if (string.IsNullOrEmpty(template))
            {
                _logger.LogWarning("Message template not found for error code: {ErrorCode}, Culture: {Culture}",
                    errorCode, _currentCulture.Name);
                return errorCode;
            }

            return args.Length > 0 ? string.Format(_currentCulture, template, args) : template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message for error code: {ErrorCode}", errorCode);
            return errorCode;
        }
    }

    public IReadOnlyList<string> GetMessages(IEnumerable<string> errorCodes)
    {
        return errorCodes.Select(code => GetMessage(code)).ToList();
    }

    public void SetCulture(string cultureName)
    {
        try
        {
            _currentCulture = new CultureInfo(cultureName);
        }
        catch (CultureNotFoundException ex)
        {
            _logger.LogError(ex, "Culture not found: {CultureName}, using default", cultureName);
            _currentCulture = CultureInfo.CurrentUICulture;
        }
    }
}
