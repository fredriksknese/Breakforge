using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Breakforge.Player;

/// <summary>
/// Persists a <see cref="PlayerProfile"/> to a JSON file under the user's
/// per-platform application-data directory. Writes are atomic (temp file +
/// replace) so a crash mid-write can't corrupt an existing save.
/// </summary>
public static class ProfileStorage
{
    public static string SaveDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Breakforge");

    public static string SavePath => Path.Combine(SaveDirectory, "profile.json");

    private static readonly JsonSerializerOptions s_opts = new() { WriteIndented = true };

    public static PlayerProfile Load()
    {
        var profile = new PlayerProfile();
        try
        {
            if (!File.Exists(SavePath)) return profile;
            var json = File.ReadAllText(SavePath);
            var dto = JsonSerializer.Deserialize<ProfileDto>(json);
            if (dto is null) return profile;

            profile.Name = dto.Name ?? profile.Name;
            profile.LifetimeScore = dto.LifetimeScore;
            profile.SkillPoints = dto.SkillPoints;
            profile.Inventory.LoadFrom(dto.Inventory);
            profile.Skills.LoadFrom(dto.UnlockedSkills);
            profile.UnlockedMaps.Clear();
            foreach (var m in dto.UnlockedMaps) profile.UnlockedMaps.Add(m);
        }
        catch
        {
            // Corrupt or unreadable save → start fresh rather than crash.
        }
        profile.ClearDirty();
        return profile;
    }

    public static void Save(PlayerProfile profile)
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var dto = ProfileDto.From(profile);
            var json = JsonSerializer.Serialize(dto, s_opts);

            var tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(SavePath)) File.Replace(tmp, SavePath, null);
            else File.Move(tmp, SavePath);

            profile.ClearDirty();
        }
        catch
        {
            // Save errors shouldn't crash the game; dirty flag stays set so the
            // next tick (or shutdown) will retry.
        }
    }

    private sealed class ProfileDto
    {
        public string? Name { get; set; }
        public Dictionary<string, int> Inventory { get; set; } = new();
        public List<string> UnlockedSkills { get; set; } = new();
        public List<string> UnlockedMaps { get; set; } = new();
        public int LifetimeScore { get; set; }
        public int SkillPoints { get; set; }

        public static ProfileDto From(PlayerProfile p)
        {
            var dto = new ProfileDto
            {
                Name = p.Name,
                LifetimeScore = p.LifetimeScore,
                SkillPoints = p.SkillPoints,
            };
            foreach (var kv in p.Inventory.Items) dto.Inventory[kv.Key] = kv.Value;
            foreach (var s in p.Skills.Unlocked) dto.UnlockedSkills.Add(s);
            foreach (var m in p.UnlockedMaps) dto.UnlockedMaps.Add(m);
            return dto;
        }
    }
}
