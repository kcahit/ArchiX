using FluentValidation;

namespace ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior
{
 public sealed class EchoRequestValidator : AbstractValidator<EchoRequest>
 {
 public EchoRequestValidator()
 {
 RuleFor(x => x.Name)
 .NotEmpty()
 .MinimumLength(2);
 }
 }
}
