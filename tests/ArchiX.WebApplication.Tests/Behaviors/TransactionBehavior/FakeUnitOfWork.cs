// File: tests/ArchiX.WebApplication.Tests/Behaviors/TransactionBehavior/FakeUnitOfWork.cs
using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.WebApplication.Tests.Behaviors.TransactionBehavior
{
    /// <summary>
    /// Test amaçlı IUnitOfWork sahte uygulaması.
    /// </summary>
    public sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }
        public CancellationToken LastToken { get; private set; }
        public bool ThrowOnSave { get; set; }
        public int ReturnValue { get; set; } = 1;

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
