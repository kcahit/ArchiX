using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Pipeline
{
 public static class ServiceCollectionExtensions
 {
 public static IServiceCollection AddArchiXAuthorizationPipeline(this IServiceCollection services) { services.AddTransient(typeof(ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<,>), typeof(ArchiX.Library.Web.Behaviors.AuthorizationBehavior<,>)); return services; }
 public static IServiceCollection AddArchiXValidationPipeline(this IServiceCollection services) { services.AddTransient(typeof(ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<,>), typeof(ArchiX.Library.Web.Behaviors.ValidationBehavior<,>)); return services; }
 public static IServiceCollection AddArchiXTransactionPipeline(this IServiceCollection services) { services.AddTransient(typeof(ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<,>), typeof(ArchiX.Library.Web.Behaviors.TransactionBehavior<,>)); return services; }
 }
}
