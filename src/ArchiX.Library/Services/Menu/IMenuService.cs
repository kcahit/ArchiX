using ArchiX.Library.Models;

namespace ArchiX.Library.Services.Menu
{
    public interface IMenuService
    {
        Task<IReadOnlyList<MenuItem>> GetMenuForApplicationAsync(int applicationId, CancellationToken ct = default);
        Task InvalidateMenuCacheAsync(int applicationId, CancellationToken ct = default);
    }
}
