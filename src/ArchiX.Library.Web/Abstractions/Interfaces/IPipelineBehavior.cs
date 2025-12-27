using ArchiX.Library.Web.Abstractions.Delegates;

namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
 {
 Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
 }
}
