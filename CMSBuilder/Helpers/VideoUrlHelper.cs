using System.Text.RegularExpressions;

namespace CMSBuilder.Helpers;

public static class VideoUrlHelper
{
    public static string ToEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        url = url.Trim();

        var ytWatch = Regex.Match(url, @"(?:youtube\.com/watch\?v=|youtube\.com/embed/)([\w-]+)", RegexOptions.IgnoreCase);
        if (ytWatch.Success)
            return $"https://www.youtube.com/embed/{ytWatch.Groups[1].Value}";

        var ytShort = Regex.Match(url, @"youtu\.be/([\w-]+)", RegexOptions.IgnoreCase);
        if (ytShort.Success)
            return $"https://www.youtube.com/embed/{ytShort.Groups[1].Value}";

        var vimeo = Regex.Match(url, @"vimeo\.com/(?:video/)?(\d+)", RegexOptions.IgnoreCase);
        if (vimeo.Success)
            return $"https://player.vimeo.com/video/{vimeo.Groups[1].Value}";

        return url;
    }
}
