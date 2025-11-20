namespace ArchiX.Library.Web.Abstractions.Delegates
{
 public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);
}
