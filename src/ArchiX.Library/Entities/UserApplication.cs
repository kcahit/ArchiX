using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Entities
{
    [Index(nameof(UserId), nameof(ApplicationId), IsUnique = true)]
    public sealed class UserApplication : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;
    }
}
