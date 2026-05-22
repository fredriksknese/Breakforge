using System.Collections.Generic;
using Breakforge.Definitions;

namespace Breakforge.Player;

/// <summary>
/// Tracks unlocked skills. Validation (prerequisites + cost) is done by the
/// SkillTreeScene before calling Unlock.
/// </summary>
public sealed class SkillTreeState
{
    private readonly HashSet<string> _unlocked = new();
    public IReadOnlySet<string> Unlocked => _unlocked;

    public bool Has(string skillId) => _unlocked.Contains(skillId);

    public bool CanUnlock(SkillNode node, PlayerProfile profile)
    {
        if (_unlocked.Contains(node.Id)) return false;
        if (profile.SkillPoints < node.Cost) return false;
        foreach (var p in node.Prerequisites)
            if (!_unlocked.Contains(p)) return false;
        return true;
    }

    public bool Unlock(SkillNode node, PlayerProfile profile)
    {
        if (!CanUnlock(node, profile)) return false;
        _unlocked.Add(node.Id);
        profile.SkillPoints -= node.Cost;
        return true;
    }
}
