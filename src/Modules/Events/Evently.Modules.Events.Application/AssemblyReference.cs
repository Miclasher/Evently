using System.Reflection;

namespace Evently.Modules.Events.Application;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
