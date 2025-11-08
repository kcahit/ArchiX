using ArchiX.Library.Web.Behaviors;

using Xunit;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 public sealed class TransactionBehaviorTests
 {
 [Fact]
 public async Task Calls_SaveChanges_On_Success()
 {
 var uow = new FakeUnitOfWork();
 var behavior = new TransactionBehavior<TxRequest, string>(uow);

 static Task<string> next(CancellationToken _) => Task.FromResult("ok");

 var result = await behavior.HandleAsync(new TxRequest("v"), next, CancellationToken.None);

 Assert.Equal("ok", result);
 Assert.Equal(1, uow.SaveChangesCalls);
 }

 [Fact]
 public async Task Does_Not_Save_When_Handler_Throws()
 {
 var uow = new FakeUnitOfWork();
 var behavior = new TransactionBehavior<TxRequest, string>(uow);

 static Task<string> next(CancellationToken _) => throw new InvalidOperationException("tx-fail");

 await Assert.ThrowsAsync<InvalidOperationException>(() =>
 behavior.HandleAsync(new TxRequest("v"), next, CancellationToken.None));

 Assert.Equal(0, uow.SaveChangesCalls);
 }

 [Fact]
 public async Task Passes_CancellationToken_To_Uow()
 {
 var uow = new FakeUnitOfWork();
 var behavior = new TransactionBehavior<TxRequest, string>(uow);

 using var cts = new CancellationTokenSource();
 var token = cts.Token;

 static Task<string> next(CancellationToken _) => Task.FromResult("ok");
 _ = await behavior.HandleAsync(new TxRequest("v"), next, token);

 Assert.Equal(token, uow.LastToken);
 }
 }
}
