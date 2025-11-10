#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Web.Security.Authorization
{
 /// <summary>Policy registration helper.</summary>
 public static class AuthorizationPolicies
 {
 public static IServiceCollection AddArchiXPolicies(this IServiceCollection services)
 {
 services.AddAuthorizationBuilder()
 .AddPolicy(PolicyNames.Admin, p => p.RequireRole("Admin"))
 .AddPolicy(PolicyNames.User, p => p.RequireRole("User"))
 .AddPolicy(PolicyNames.CanExport, p => p.RequireClaim(ClaimTypesEx.Permission, "export"))
 .AddPolicy(PolicyNames.CanImport, p => p.RequireClaim(ClaimTypesEx.Permission, "import"));
 return services;
 }
 }
}
