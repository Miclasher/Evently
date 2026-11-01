namespace Evently.Modules.Users.Domain.Users;

public sealed class Role
{
    public static readonly Role Administrator = new("Administrator");
    public static readonly Role Member = new("Member");
    public static readonly Role Premium = new("Premium");

    private Role(string name)
    {
        Name = name;
    }

    private Role()
    {
    }

    public string Name { get; private set; }
}
