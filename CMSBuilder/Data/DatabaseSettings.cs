using System.IO;
using Newtonsoft.Json.Linq;

namespace CMSBuilder.Data;

public static class DatabaseSettings
{
    public const string DefaultLocalDbConnection =
        "Server=(localdb)\\mssqllocaldb;Database=CmsBuilder;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public static string GetConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CMSBUILDER_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv.Trim();

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            try
            {
                var json = JObject.Parse(File.ReadAllText(path));
                var cs = json["ConnectionStrings"]?["DefaultConnection"]?.Value<string>();
                if (!string.IsNullOrWhiteSpace(cs))
                    return cs.Trim();
            }
            catch
            {
                // fallback below
            }
        }

        return DefaultLocalDbConnection;
    }
}
