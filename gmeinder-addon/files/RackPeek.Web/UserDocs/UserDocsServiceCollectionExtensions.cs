using RackPeek.Web.UserDocs;
using Shared.Rcl.Docs;

namespace RackPeek.Web;

public static class UserDocsServiceCollectionExtensions {
    /// <summary>
    ///     Adds "My Docs": Markdown pages in {configDir}/docs, served at docs/my/{name}.
    ///     Registered after the built-in provider, so it wraps it.
    /// </summary>
    public static IServiceCollection AddUserDocs(this IServiceCollection services, string configDir) {
        var directory = Path.Combine(configDir, "docs");
        Directory.CreateDirectory(directory);

        services.AddSingleton<IUserDocsStore>(new FileUserDocsStore(directory));
        services.AddScoped<StaticWebAssetDocsContentProvider>();
        services.AddScoped<IDocsContentProvider, UserDocsContentProvider>();
        return services;
    }
}
