using ArchiX.Library.Abstractions.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 public sealed class FakeUnitOfWork : IUnitOfWork
 {
 public int SaveChangesCalls { get; private set; }
 public CancellationToken LastToken { get; private set; }
 public bool ThrowOnSave { get; set; }
 public int ReturnValue { get; set; } =1;

 public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
 {
 LastToken = cancellationToken;
 if (ThrowOnSave)
 throw new InvalidOperationException("fake-uow-save-failed");
 SaveChangesCalls++;
 return Task.FromResult(ReturnValue);
 }
 }
}
