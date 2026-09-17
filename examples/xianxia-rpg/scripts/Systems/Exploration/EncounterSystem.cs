using System.Collections.Generic;
using System.Linq;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Systems.Exploration;

/// <summary>
/// SYS-016/017 遭遇系统（最小契约）：顺序推进区域内的遭遇点，
/// 全部清完进入 EndNode。确定性、可回放，便于 NUnit 断言。
/// </summary>
public class EncounterSystem
{
    private readonly Region _region;
    private int _index;

    public EncounterSystem(Region region) => _region = region;

    public bool IsComplete => _region.Spots.All(s => s.Cleared);

    public EncounterSpot CurrentSpot() => _index < _region.Spots.Count ? _region.Spots[_index] : null;

    /// <summary>开始下一场战斗，返回该遭遇点的敌人组；已无则 null。</summary>
    public List<Combatant> StartNextEncounter() => CurrentSpot()?.Enemies;

    /// <summary>标记当前遭遇点已清，推进到下一个。</summary>
    public void MarkCleared()
    {
        var spot = CurrentSpot();
        if (spot != null)
        {
            spot.Cleared = true;
            _index++;
        }
    }

    /// <summary>区域全清后返回剧情节点，否则 null。</summary>
    public StoryNode ReachedEndNode() => IsComplete ? _region.EndNode : null;
}
