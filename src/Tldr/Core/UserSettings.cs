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
    public string Voice { get; set; } = "";

    public static UserSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new UserSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch (Exception ex)
        {
            // Corrupt settings file; reset to defaults
            System.Diagnostics.Debug.WriteLine($"[Settings] Load failed, using defaults: {ex.Message}");
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
        catch (Exception ex)
        {
            // Non-fatal: settings not persisted this time
            System.Diagnostics.Debug.WriteLine($"[Settings] Save failed: {ex.Message}");
        }
    }
}
