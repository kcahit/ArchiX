namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface IUnitOfWork
 {
 Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
 }
}
