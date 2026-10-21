namespace Shared.Rcl.Docs;

/// <summary>
///     User-supplied Markdown pages ("My Docs") kept next to the config, managed from the
///     Docs sidebar and served at docs/my/{name}.
/// </summary>
public interface IUserDocsStore {
    public const string RoutePrefix = "my";

    IReadOnlyList<string> List();

    Task<string> GetAsync(string fileName);

    /// <summary>Stores the upload under a sanitised .md name (an existing page is replaced) and returns that name.</summary>
    Task<string> SaveAsync(string fileName, Stream content);

    /// <summary>Renames a page to a sanitised .md name and returns that name; fails if the name is taken.</summary>
    Task<string> RenameAsync(string fileName, string newName);

    Task DeleteAsync(string fileName);
}
