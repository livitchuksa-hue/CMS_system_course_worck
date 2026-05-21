using System.IO;
using CMSBuilder.Data;
using CMSBuilder.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CMSBuilder.Services;

public class AppSettingsData
{
    public bool RememberMe { get; set; }
    public int? UserId { get; set; }
    public string Login { get; set; } = string.Empty;
}

public class AppSettingsService
{
    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CMSBuilder", "settings.json");

    public AppSettingsData Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new AppSettingsData();
            var json = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<AppSettingsData>(json) ?? new AppSettingsData();
        }
        catch
        {
            return new AppSettingsData();
        }
    }

    public void Save(AppSettingsData data)
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public void Clear() => Save(new AppSettingsData());

    public User? TryRestoreUser()
    {
        var s = Load();
        if (!s.RememberMe || s.UserId == null) return null;
        using var db = new AppDbContext();
        return db.Users.FirstOrDefault(u => u.Id == s.UserId);
    }

    public void Remember(User user)
    {
        Save(new AppSettingsData
        {
            RememberMe = true,
            UserId = user.Id,
            Login = user.Login
        });
    }
}
