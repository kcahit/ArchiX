using System.Reflection;

using ArchiX.WebApplication.Abstractions.Authorizations;
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.WebApplication.Behaviors
{
    /// <summary>
    /// İstek tipi üzerindeki <see cref="AuthorizeAttribute"/> meta verisine göre yetkilendirme uygular.
    /// Politikalar yoksa akışı devam ettirir; başarısızlıkta <see cref="UnauthorizedAccessException"/> fırlatır.
    /// </summary>
    /// <typeparam name="TRequest">İstek tipi.</typeparam>
    /// <typeparam name="TResponse">Yanıt tipi.</typeparam>
    public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationBehavior(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        /// <inheritdoc />
        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var attr = typeof(TRequest).GetCustomAttribute<AuthorizeAttribute>(inherit: true);

            if (attr is null || attr.Policies.Count == 0)
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }

            var authorized = await InvokeAuthorizationAsync(
                    _authorizationService,
                    attr.Policies,
                    attr.RequireAll,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!authorized)
            {
                var policyList = string.Join(",", attr.Policies);
                throw new UnauthorizedAccessException(
                    $"Authorization failed for '{typeof(TRequest).Name}' with policies [{policyList}].");
            }

            return await next(cancellationToken).ConfigureAwait(false);
        }

        private static async Task<bool> InvokeAuthorizationAsync(
            IAuthorizationService service,
            IReadOnlyList<string> policies,
            bool requireAll,
            CancellationToken cancellationToken)
        {
            var type = service.GetType();

            // Desteklenen yöntem adları
            var candidateNames = new[] { "AuthorizeAsync", "IsAuthorizedAsync" };

            // 3 parametreli imza: (IEnumerable<string>/string[], bool, CancellationToken)
            foreach (var name in candidateNames)
            {
                var m = type.GetMethod(name, new[] { typeof(IReadOnlyList<string>), typeof(bool), typeof(CancellationToken) })
                        ?? type.GetMethod(name, new[] { typeof(IEnumerable<string>), typeof(bool), typeof(CancellationToken) })
                        ?? type.GetMethod(name, new[] { typeof(string[]), typeof(bool), typeof(CancellationToken) });

                if (m != null)
                {
                    object argPolicies = policies;
                    if (m.GetParameters()[0].ParameterType == typeof(string[]))
                    {
                        var arr = new string[policies.Count];
                        for (var i = 0; i < policies.Count; i++) arr[i] = policies[i];
                        argPolicies = arr;
                    }

                    var result = m.Invoke(service, new object[] { argPolicies, requireAll, cancellationToken });
                    if (result is Task<bool> tb) return await tb.ConfigureAwait(false);
                    if (result is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var pr = t.GetType().GetProperty("Result");
                        if (pr?.PropertyType == typeof(bool) && pr.GetValue(t) is bool b) return b;
                        return true; // Task tamamlandıysa true varsay.
                    }
                    if (result is bool b1) return b1;
                }
            }

            // 2 parametreli imza: (IEnumerable<string>/string[], CancellationToken) — requireAll parametresi yok
            foreach (var name in candidateNames)
            {
                var m = type.GetMethod(name, new[] { typeof(IReadOnlyList<string>), typeof(CancellationToken) })
                        ?? type.GetMethod(name, new[] { typeof(IEnumerable<string>), typeof(CancellationToken) })
                        ?? type.GetMethod(name, new[] { typeof(string[]), typeof(CancellationToken) });

                if (m != null)
                {
                    object argPolicies = policies;
                    if (m.GetParameters()[0].ParameterType == typeof(string[]))
                    {
                        var arr = new string[policies.Count];
                        for (var i = 0; i < policies.Count; i++) arr[i] = policies[i];
                        argPolicies = arr;
                    }

                    var result = m.Invoke(service, new object[] { argPolicies, cancellationToken });
                    if (result is Task<bool> tb) return await tb.ConfigureAwait(false);
                    if (result is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var pr = t.GetType().GetProperty("Result");
                        if (pr?.PropertyType == typeof(bool) && pr.GetValue(t) is bool b) return b;
                        return true;
                    }
                    if (result is bool b1) return b1;
                }
            }

            throw new MissingMethodException(
                $"{type.FullName} üzerinde desteklenen bir yetkilendirme yöntemi bulunamadı. " +
                "Beklenen örnek imzalar: " +
                "Task<bool> AuthorizeAsync(IEnumerable<string> policies, bool requireAll, CancellationToken ct) " +
                "veya Task<bool> AuthorizeAsync(IEnumerable<string> policies, CancellationToken ct).");
        }
    }
}
