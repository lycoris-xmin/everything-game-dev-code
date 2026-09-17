using System.Collections.Generic;
using XianxiaRPG.Systems.Battle;
using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Entities;

/// <summary>
/// SYS-008 战斗单位（玩家角色/敌人共用）。纯数据 + 轻量方法，无引擎依赖。
/// 资源：精力(HP) / 气力(MP) / 神力(Ultimate 槽) / 韧性(Toughness)。
/// </summary>
public class Combatant
{
    public string Name = "?";
    public bool IsEnemy;

    public Element Element;
    public AttributeSet Attr = new();

    public int MaxHp = 100;
    public int Hp = 100;
    public int MaxMp = 50;
    public int Mp = 50;

    public int MaxToughness = 100;
    public int Toughness = 100;

    public int Ultimate; // 神力槽 0..100
    public int Av;       // 行动值累积
    public bool Defending;
    public int BrokenTurns; // >0 表示击破中（持续该单位行动次数）

    public List<Skill> Skills = new();

    public bool Alive => Hp > 0;
    public int Speed => Attr[AttrKey.Speed];

    public void AddSkill(Skill s) => Skills.Add(s);

    public Combatant Clone()
    {
        var c = new Combatant
        {
            Name = Name, IsEnemy = IsEnemy, Element = Element,
            MaxHp = MaxHp, Hp = Hp, MaxMp = MaxMp, Mp = Mp,
            MaxToughness = MaxToughness, Toughness = Toughness,
            Ultimate = Ultimate, Av = Av, Defending = Defending, BrokenTurns = BrokenTurns
        };
        c.Attr = Attr.Clone();
        c.Skills = new List<Skill>(Skills);
        return c;
    }
}
