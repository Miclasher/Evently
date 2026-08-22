namespace Evently.Api.Extensions;

internal static class ConfigurationExtensions
{
    public static void AddModuleConfiguration(this IConfigurationBuilder builder, string[] modules)
    {
        foreach (string module in modules)
        {
            builder.AddJsonFile($"modules.{module}.json", false, true);
            builder.AddJsonFile($"modules.{module}.Development.json", true, true);
        }
    }    
}
