using Evently.Common.Domain;

namespace Evently.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.NotFound", $"The user with the identifier {userId} not found");

    public static Error NotFound(string identityId) =>
        Error.NotFound("Users.NotFound", $"The user with the IDP identifier {identityId} not found");

    public static Error PlanNotFound(string plan) =>
        Error.NotFound("Users.PlanNotFound", $"The plan {plan} was not found");
}
