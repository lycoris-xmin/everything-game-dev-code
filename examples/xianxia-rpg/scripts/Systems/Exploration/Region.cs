using System.Collections.Generic;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Systems.Exploration;

/// <summary>
/// SYS-016/017/019 探索（最小契约）：区域 + 遭遇点 + 剧情节点。
/// 纯数据模型，场景集成（移动/Area2D 触发）在 Godot 层后续接入。
/// </summary>
public class EncounterSpot
{
    public int Id;
    public string Name = "";
    public List<Combatant> Enemies = new();
    public bool Cleared;
}

public class StoryNode
{
    public string Id = "";
    public string Text = "";
}

public class Region
{
    public string Id = "";
    public string Name = "";
    public List<EncounterSpot> Spots = new();
    public StoryNode EndNode; // 区域通关后进入的剧情节点
}
