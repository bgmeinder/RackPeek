using Shared.Rcl.Docs;

namespace RackPeek.Web.UserDocs;

/// <summary>
///     Serves "my/{name}.md" from the user docs store and everything else from the
///     built-in docs, so the Docs page needs no changes to show user pages.
/// </summary>
public class UserDocsContentProvider(
    StaticWebAssetDocsContentProvider builtIn,
    IUserDocsStore userDocs) : IDocsContentProvider {
    public Task<string> GetAsync(string fileName) {
        var prefix = IUserDocsStore.RoutePrefix + "/";
        return fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? userDocs.GetAsync(fileName[prefix.Length..])
            : builtIn.GetAsync(fileName);
    }
}
