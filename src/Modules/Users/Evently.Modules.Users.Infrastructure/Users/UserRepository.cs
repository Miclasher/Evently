using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Role?> GetRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Set<Role>().SingleOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            context.Attach(role);
        }

        context.Users.Add(user);
    }
}
