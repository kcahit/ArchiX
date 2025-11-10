#nullable enable
using ArchiX.Library.Abstractions.Security;
using System.Text.RegularExpressions;

namespace ArchiX.Library.Services.Security
{
 public sealed class MaskingService : IMaskingService
 {
 public string Mask(string? input, int unmaskedPrefix =2, int unmaskedSuffix =2, char maskChar = '*')
 {
 if (string.IsNullOrEmpty(input)) return string.Empty;
 if (input.Length <= unmaskedPrefix + unmaskedSuffix) return new string(maskChar, input.Length);
 var prefix = input.Substring(0, unmaskedPrefix);
 var suffix = input.Substring(input.Length - unmaskedSuffix);
 return prefix + new string(maskChar, input.Length - unmaskedPrefix - unmaskedSuffix) + suffix;
 }

 public string MaskEmail(string? email)
 {
 if (string.IsNullOrWhiteSpace(email)) return string.Empty;
 var parts = email.Split('@');
 if (parts.Length !=2) return Mask(email);
 var local = parts[0];
 var domain = parts[1];
 var maskedLocal = Mask(local,1,1);
 return maskedLocal + "@" + domain;
 }

 public string MaskPhone(string? phone)
 {
 if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
 var digits = Regex.Replace(phone, "[^0-9]", "");
 return Mask(digits,2,2);
 }
 }
}
