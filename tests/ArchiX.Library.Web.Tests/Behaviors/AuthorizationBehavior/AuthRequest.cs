using ArchiX.Library.Web.Abstractions.Authorizations;
using ArchiX.Library.Web.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.AuthorizationBehavior
{
 [Authorize(ArchiX.Library.Web.Abstractions.Authorizations.AuthorizePolicies.AdminOnly)]
 public sealed record AuthRequest(string Value) : IRequest<string>;
}
