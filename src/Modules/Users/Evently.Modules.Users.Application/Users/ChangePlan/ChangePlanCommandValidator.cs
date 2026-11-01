using Evently.Modules.Users.Domain.Users;
using FluentValidation;

namespace Evently.Modules.Users.Application.Users.ChangePlan;

internal sealed class ChangePlanCommandValidator : AbstractValidator<ChangePlanCommand>
{
    public ChangePlanCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Plan)
            .Must(p => p == Role.Member.Name || p == Role.Premium.Name)
            .WithMessage($"Plan must be '{Role.Member.Name}' or '{Role.Premium.Name}'.");
    }
}
