#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 public interface IMaskingService
 {
 string Mask(string? input, int unmaskedPrefix =2, int unmaskedSuffix =2, char maskChar = '*');
 string MaskEmail(string? email);
 string MaskPhone(string? phone);
 }
}
