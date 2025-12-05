namespace ArchiX.Library.Web.Abstractions.Interfaces
{
 public interface IMediator
 {
 Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
 }
}
