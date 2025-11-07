using ArchiX.WebApplication.Abstractions.Authorizations;
using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.AuthorizationBehavior
{
 [Authorize(ArchiX.WebApplication.Abstractions.Authorizations.AuthorizePolicies.AdminOnly)]
 public sealed record AuthRequest(string Value) : IRequest<string>;
}
