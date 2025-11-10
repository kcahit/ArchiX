#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>Uygulama özel claim type sabitleri.</summary>
 public static class ClaimTypesEx
 {
 public const string Permission = "permission"; // scope benzeri ama daha ince yetki
 public const string TenantId = "tenant";
 public const string DisplayName = "display_name";
 }
}
