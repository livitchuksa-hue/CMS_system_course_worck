namespace CMSBuilder.Helpers;

public static class ProjectFonts
{
    public static IReadOnlyList<string> All { get; } = new[]
    {
        "Segoe UI",
        "Arial",
        "Times New Roman",
        "Georgia",
        "Verdana",
        "Tahoma",
        "Inter",
        "Roboto",
        "Open Sans",
        "Montserrat"
    };

    public static string ToCssFamily(string font) =>
        font.Contains(',') ? font : $"\"{font}\", sans-serif";
}
