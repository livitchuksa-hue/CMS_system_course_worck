namespace CMSBuilder.Helpers;

public static class FileHelper
{
    public static string GetExportsRoot()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Exports");
        Directory.CreateDirectory(path);
        return path;
    }

    public static string GetWebsiteExportFolder(int websiteId, string slug)
    {
        var safe = string.IsNullOrWhiteSpace(slug) ? $"site-{websiteId}" : slug;
        foreach (var c in Path.GetInvalidFileNameChars())
            safe = safe.Replace(c, '-');
        var path = Path.Combine(GetExportsRoot(), safe);
        Directory.CreateDirectory(path);
        return path;
    }
}
