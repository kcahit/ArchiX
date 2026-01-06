namespace ArchiX.Library.Runtime.Reports;

internal static class ReportDatasetFilePathResolver
{
    public static string ResolveAndValidate(string root, string? subPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("File dataset root is missing.");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("FileName is missing.");

        if (Path.IsPathRooted(fileName))
            throw new InvalidOperationException("Absolute paths are not allowed.");

        var cleanSub = (subPath ?? string.Empty).Replace('\\', '/').Trim();
        if (cleanSub.StartsWith('/')) cleanSub = cleanSub.TrimStart('/');

        var combined = Path.Combine(root, cleanSub, fileName);
        var full = Path.GetFullPath(combined);
        var rootFull = Path.GetFullPath(root);

        if (!rootFull.EndsWith(Path.DirectorySeparatorChar))
            rootFull += Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path traversal detected.");

        return full;
    }
}
