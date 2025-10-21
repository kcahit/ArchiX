// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidationBehavior/EchoRequestValidator.cs
using FluentValidation;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidationBehavior
{
    /// <summary>
    /// EchoRequest için doğrulayıcı.
    /// </summary>
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
