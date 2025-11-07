using ArchiX.Library.Web.Abstractions.Interfaces;
using ArchiX.Library.Web.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Pipeline
{
 public static class CqrsRegistrationExtensions
 {
 public static IServiceCollection AddArchiXCqrs(this IServiceCollection services)
 {
 services.AddSingleton<IMediator, Mediator>();
 services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
 services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
 services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
 return services;
 }
 public static IServiceCollection AddArchiXCqrs(this IServiceCollection services, System.Reflection.Assembly assembly)
 {
 _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
 return services.AddArchiXCqrs();
 }
 }
}
