#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 /// <summary>Rate limiting policy adlarý.</summary>
 public static class RateLimitPolicyNames
 {
 public const string Anonymous = "rl.anonymous";
 public const string Authenticated = "rl.auth";
 public const string Login = "rl.login";
 public const string ExportHeavy = "rl.export_heavy";
 }
}
