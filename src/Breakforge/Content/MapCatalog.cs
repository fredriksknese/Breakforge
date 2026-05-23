using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Breakforge.Definitions;

namespace Breakforge.Content;

/// <summary>
/// Loads levels and maps from JSON .map files embedded as resources under
/// Maps/. Each file describes one level plus a small "map" block; files that
/// share a map id are bundled into a single MapDef. Add new content by
/// dropping a .map JSON into src/Breakforge/Maps/ — the csproj globs them in.
/// </summary>
public static class MapCatalog
{
    private const string ResourcePrefix = "Maps/";
    private const string ResourceSuffix = ".map";

    private static readonly JsonSerializerOptions s_opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static void Register(Registry r)
    {
        var asm = typeof(MapCatalog).Assembly;
        var resources = asm.GetManifestResourceNames()
            .Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal)
                     && n.EndsWith(ResourceSuffix, StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var parsed = new List<LevelFile>(resources.Count);
        foreach (var name in resources)
        {
            var file = LoadResource(asm, name);
            if (file is null) continue;
            r.Register(BuildLevelDef(file));
            parsed.Add(file);
        }

        // Group by map id; first file that declares the map wins for metadata.
        var mapMeta = new Dictionary<string, MapBlock>();
        var mapMembers = new Dictionary<string, List<LevelFile>>();
        foreach (var f in parsed)
        {
            if (f.Map is null) continue;
            if (!mapMeta.ContainsKey(f.Map.Id)) mapMeta[f.Map.Id] = f.Map;
            if (!mapMembers.TryGetValue(f.Map.Id, out var list))
                mapMembers[f.Map.Id] = list = new List<LevelFile>();
            list.Add(f);
        }

        foreach (var (mapId, levels) in mapMembers)
        {
            var ordered = levels
                .OrderBy(l => l.Order ?? int.MaxValue)
                .ThenBy(l => l.Id, StringComparer.Ordinal)
                .Select(l => l.Id)
                .ToList();
            var m = mapMeta[mapId];
            r.Register(new MapDef
            {
                Id = m.Id,
                DisplayName = m.DisplayName,
                Subtitle = m.Subtitle,
                LevelIds = ordered,
                LockedByDefault = m.LockedByDefault,
                CompletionReward = m.CompletionReward,
            });
        }
    }

    private static LevelFile? LoadResource(Assembly asm, string name)
    {
        using var stream = asm.GetManifestResourceStream(name);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        try { return JsonSerializer.Deserialize<LevelFile>(json, s_opts); }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Failed to parse map resource '{name}': {ex.Message}", ex);
        }
    }

    private static LevelDef BuildLevelDef(LevelFile f)
    {
        if (string.IsNullOrWhiteSpace(f.Id))
            throw new InvalidDataException("Map file missing 'id'.");

        var legend = new Dictionary<char, string>(f.Legend.Count);
        foreach (var kv in f.Legend)
        {
            if (kv.Key.Length != 1)
                throw new InvalidDataException(
                    $"Level '{f.Id}': legend keys must be single characters (got '{kv.Key}').");
            legend[kv.Key[0]] = kv.Value;
        }

        return new LevelDef
        {
            Id = f.Id,
            DisplayName = f.DisplayName ?? f.Id,
            Legend = legend,
            Rows = f.Rows,
            BackgroundColor = ParseColor(f.BackgroundColor, new Color(10, 12, 24)),
            WallColor = ParseColor(f.WallColor, new Color(40, 44, 60)),
            PowerupDropMultiplier = f.PowerupDropMultiplier ?? 1f,
        };
    }

    private static Color ParseColor(int[]? rgb, Color fallback)
    {
        if (rgb is null) return fallback;
        if (rgb.Length is < 3 or > 4)
            throw new InvalidDataException("Color must be [r,g,b] or [r,g,b,a].");
        byte a = rgb.Length == 4 ? (byte)rgb[3] : (byte)255;
        return new Color((byte)rgb[0], (byte)rgb[1], (byte)rgb[2], a);
    }

    private sealed class LevelFile
    {
        public string Id { get; set; } = "";
        public string? DisplayName { get; set; }
        public int? Order { get; set; }
        public MapBlock? Map { get; set; }
        public Dictionary<string, string> Legend { get; set; } = new();
        public List<string> Rows { get; set; } = new();
        public int[]? BackgroundColor { get; set; }
        public int[]? WallColor { get; set; }
        public float? PowerupDropMultiplier { get; set; }
    }

    private sealed class MapBlock
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public bool LockedByDefault { get; set; }
        public string? CompletionReward { get; set; }
    }
}
