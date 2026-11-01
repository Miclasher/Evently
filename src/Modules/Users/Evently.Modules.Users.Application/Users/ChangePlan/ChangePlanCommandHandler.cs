using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Users.Application.Abstractions.Data;
using Evently.Modules.Users.Domain.Users;

namespace Evently.Modules.Users.Application.Users.ChangePlan;

internal sealed class ChangePlanCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ChangePlanCommand>
{
    public async Task<Result> Handle(ChangePlanCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        Role? plan = await userRepository.GetRoleAsync(request.Plan, cancellationToken);

        if (plan is null)
        {
            return Result.Failure(UserErrors.PlanNotFound(request.Plan));
        }

        user.ChangePlan(plan);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
