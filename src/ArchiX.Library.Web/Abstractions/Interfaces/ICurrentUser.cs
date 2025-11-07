namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface ICurrentUser
 {
 string? UserId { get; }
 string? UserName { get; }
 bool IsAuthenticated { get; }
 IReadOnlyCollection<string> Roles { get; }
 bool IsInRole(string role);
 }
}
