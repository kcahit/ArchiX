namespace ArchiX.WebApplication.Abstractions.Authorizations
{
    /// <summary>
    /// Komut/sorgu istek tiplerine uygulanacak yetkilendirme meta verisini tanımlar.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Bir veya daha fazla politika adı ile başlatır.
        /// </summary>
        /// <param name="policies">Değerlendirilecek politika adları.</param>
        public AuthorizeAttribute(params string[] policies)
        {
            Policies = policies ?? [];
        }

        /// <summary>
        /// Değerlendirilecek politika adları.
        /// </summary>
        public IReadOnlyList<string> Policies { get; }

        /// <summary>
        /// Tüm politikalar başarılı olmalı mı.
        /// true ise tümü; false ise herhangi biri yeterlidir.
        /// Varsayılan: true.
        /// </summary>
        public bool RequireAll { get; set; } = true;
    }
}
