namespace ArchiX.WebApplication.Abstractions.Authorizations
{
    /// <summary>
    /// Uygulama genelinde kullanılacak standart yetkilendirme politika adları.
    /// Testlerde de bu sabitler referans alınır.
    /// </summary>
    public static class AuthorizePolicies
    {
        /// <summary>
        /// Kimliği doğrulanmış herhangi bir kullanıcı.
        /// </summary>
        public const string AnyAuthenticated = "auth:any";

        /// <summary>
        /// Yalnızca yönetici ayrıcalığı gerektiren işlemler.
        /// </summary>
        public const string AdminOnly = "auth:admin";
    }
}
