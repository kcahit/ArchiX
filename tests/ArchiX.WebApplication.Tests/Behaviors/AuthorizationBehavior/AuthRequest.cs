using ArchiX.WebApplication.Abstractions.Authorizations;
using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.WebApplication.Tests.Behaviors.AuthorizationBehavior
{
    [Authorize(AuthorizePolicies.AdminOnly)]
    public sealed record AuthRequest(string Value) : IRequest<string>;
}
