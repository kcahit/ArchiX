namespace ArchiX.Library.Web.Abstractions.Authorizations
{
 [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
 public sealed class AuthorizeAttribute : System.Attribute
 {
 public AuthorizeAttribute(params string[] policies) { Policies = policies ?? System.Array.Empty<string>(); }
 public IReadOnlyList<string> Policies { get; }
 public bool RequireAll { get; set; } = true;
 }
}
