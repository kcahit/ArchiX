#nullable enable
namespace ArchiX.Library.Abstractions.Security
{
 public sealed class TwoFactorOptions
 {
 public bool Enabled { get; set; } = true;
 public TwoFactorChannel PreferredChannel { get; set; } = TwoFactorChannel.Email;
 public int CodeLength { get; set; } =6;
 public int CodeExpirySeconds { get; set; } =300; //5 minutes
 public int MaxAttemptsPerWindow { get; set; } =5;
 public int AttemptWindowSeconds { get; set; } =60;
 public int RecoveryCodeCount { get; set; } =10;
 }
}
