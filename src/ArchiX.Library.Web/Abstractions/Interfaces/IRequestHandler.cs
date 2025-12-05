namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
 {
 Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
 }
}
