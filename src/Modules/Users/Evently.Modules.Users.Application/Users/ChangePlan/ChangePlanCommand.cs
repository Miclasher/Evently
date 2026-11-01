using Evently.Common.Application.Messaging;

namespace Evently.Modules.Users.Application.Users.ChangePlan;

public sealed record ChangePlanCommand(Guid UserId, string Plan) : ICommand;
