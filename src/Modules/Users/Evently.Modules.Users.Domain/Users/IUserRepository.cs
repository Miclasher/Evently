namespace Evently.Modules.Users.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleAsync(string name, CancellationToken cancellationToken = default);

    void Insert(User user);
}
