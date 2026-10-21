using System.Text.RegularExpressions;
using Shared.Rcl.Docs;

namespace RackPeek.Web.UserDocs;

public partial class FileUserDocsStore(string directory) : IUserDocsStore {
    public IReadOnlyList<string> List() {
        if (!Directory.Exists(directory))
            return [];

        return Directory.EnumerateFiles(directory, "*.md")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<string> GetAsync(string fileName) {
        var path = Resolve(fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"User doc '{fileName}' not found.");

        return await File.ReadAllTextAsync(path);
    }

    public async Task<string> SaveAsync(string fileName, Stream content) {
        var name = Sanitize(fileName);
        Directory.CreateDirectory(directory);

        var target = Resolve(name);
        var temp = target + ".upload";
        await using (FileStream file = File.Create(temp)) {
            await content.CopyToAsync(file);
        }

        File.Move(temp, target, overwrite: true);
        return name;
    }

    public Task<string> RenameAsync(string fileName, string newName) {
        var source = Resolve(fileName);
        if (!File.Exists(source))
            throw new FileNotFoundException($"User doc '{fileName}' not found.");

        var name = Sanitize(newName);
        if (string.Equals(name, fileName, StringComparison.Ordinal))
            return Task.FromResult(name);

        var target = Resolve(name);
        if (File.Exists(target) && !string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase))
            throw new IOException($"A page named '{Path.GetFileNameWithoutExtension(name)}' already exists.");

        File.Move(source, target);
        return Task.FromResult(name);
    }

    public Task DeleteAsync(string fileName) {
        File.Delete(Resolve(fileName));
        return Task.CompletedTask;
    }

    private string Resolve(string fileName) {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name) || name != fileName ||
            !name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            throw new FileNotFoundException($"Invalid user doc name '{fileName}'.");

        return Path.Combine(directory, name);
    }

    private static string Sanitize(string fileName) {
        var stem = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        stem = UnsafeChars().Replace(stem.Replace(' ', '-'), "").Trim('-');
        if (stem.Length == 0)
            throw new ArgumentException("The file name has no usable characters.", nameof(fileName));

        return stem + ".md";
    }

    [GeneratedRegex("[^a-z0-9_-]")]
    private static partial Regex UnsafeChars();
}
