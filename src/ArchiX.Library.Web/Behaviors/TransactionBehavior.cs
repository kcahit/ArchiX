using ArchiX.Library.Web.Abstractions.Delegates;
using ArchiX.Library.Web.Abstractions.Interfaces;
using UnitOfWorkContract = ArchiX.Library.Abstractions.Persistence.IUnitOfWork;

namespace ArchiX.Library.Web.Behaviors
{
 public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
 {
 private readonly UnitOfWorkContract _uow;
 public TransactionBehavior(UnitOfWorkContract uow) => _uow = uow;
 public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
 {
 var res = await next(cancellationToken).ConfigureAwait(false);
 await _uow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
 return res;
 }
 }
}
