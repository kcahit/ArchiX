#nullable enable
using System.Security.Claims;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Security
{
 public class AuthorizationPoliciesTests
 {
 private static readonly Claim[] ExportPermission = [new(ClaimTypesEx.Permission, "export")];

 [Fact]
 public async Task AdminPolicy_Requires_Admin_Role()
 {
 var s = new ServiceCollection();
 s.AddLogging();
 s.AddAuthorization();
 s.AddArchiXPolicies();
 var sp = s.BuildServiceProvider();
 var auth = sp.GetRequiredService<IAuthorizationService>();
 var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], "test"));
 var result = await auth.AuthorizeAsync(user, null, PolicyNames.Admin);
 Assert.True(result.Succeeded);
 }

 [Fact]
 public async Task CanExport_Requires_Permission_Claim()
 {
 var s = new ServiceCollection();
 s.AddLogging();
 s.AddAuthorization();
 s.AddArchiXPolicies();
 var sp = s.BuildServiceProvider();
 var auth = sp.GetRequiredService<IAuthorizationService>();
 var user = new ClaimsPrincipal(new ClaimsIdentity(ExportPermission, "test"));
 var result = await auth.AuthorizeAsync(user, null, PolicyNames.CanExport);
 Assert.True(result.Succeeded);
 }
 }
}
