using System.IO;
using System.Text.Json;

namespace Tldr.Core;

public sealed class UserSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Tldr", "settings.json");

    public string Style { get; set; } = "Bullets";
    public string Detail { get; set; } = "Standard";
    public string Tone { get; set; } = "Neutral";
    public string Theme { get; set; } = "System";

    public static UserSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new UserSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch
        {
            // Corrupt settings file; reset to defaults
            return new UserSettings();
        }
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Non-fatal: settings not persisted this time
            System.Diagnostics.Debug.WriteLine("[Settings] Save failed (disk full or permission denied)");
        }
    }
}
