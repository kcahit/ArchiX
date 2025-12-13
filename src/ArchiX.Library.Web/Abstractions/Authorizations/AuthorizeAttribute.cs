namespace ArchiX.Library.Web.Abstractions.Authorizations
{
    /// <summary>Policy tabanlý basit authorize attribute (pipeline ve Razor Pages için).</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public sealed class AuthorizeAttribute : Attribute
    {
        /// <summary>Eski kullaným için tekil policy adý (ilk öðe yansýtýlýr).</summary>
        public string Policy { get; set; } = string.Empty;

        /// <summary>Birden fazla policy adý.</summary>
        public List<string> Policies { get; } = new();

        /// <summary>Tüm policy’ler gerekli mi? (true=AND, false=OR). Varsayýlan: true</summary>
        public bool RequireAll { get; set; } = true;

        public AuthorizeAttribute()
        {
        }

        public AuthorizeAttribute(string policy)
        {
            if (!string.IsNullOrWhiteSpace(policy))
            {
                Policy = policy;
                Policies.Add(policy);
            }
        }

        public AuthorizeAttribute(params string[] policies)
        {
            if (policies is { Length: > 0 })
            {
                Policies.AddRange(policies.Where(p => !string.IsNullOrWhiteSpace(p))!);
                Policy = Policies.FirstOrDefault() ?? string.Empty;
            }
        }
    }
}
