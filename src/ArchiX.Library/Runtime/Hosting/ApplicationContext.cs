using ArchiX.Library.Abstractions.Hosting;

namespace ArchiX.Library.Runtime.Hosting
{
    /// <summary>Basit uygulama bağlamı (scoped).</summary>
    public sealed class ApplicationContext : IApplicationContext
    {
        public int ApplicationId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
