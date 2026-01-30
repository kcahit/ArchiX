namespace ArchiX.Library.Abstractions.Hosting
{
    /// <summary>Geçerli istek için ApplicationId ve kullanıcı bilgilerini taşır.</summary>
    public interface IApplicationContext
    {
        int ApplicationId { get; set; }
        int? UserId { get; set; }
        string? UserName { get; set; }
    }
}
