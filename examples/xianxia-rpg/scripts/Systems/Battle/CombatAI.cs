using System.Collections.Generic;
using System.Linq;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Systems.Battle;

/// <summary>
/// SYS-010 AI 接口。M2 用确定性策略（无随机），便于回放与测试。
/// </summary>
public interface ICombatAI
{
    bool WantsUltimate(Combatant actor);
    Skill ChooseSkill(Combatant actor);
    Combatant ChooseTarget(Combatant actor, List<Combatant> foes);
}

/// <summary>
/// 简单 AI：神力满则终结技；否则优先可付 MP 的仙术，否则普攻；集火 HP 最低敌人。
/// 玩家与敌人 M2 共用（确定性手动策略的占位）。
/// </summary>
public class SimpleAI : ICombatAI
{
    public bool WantsUltimate(Combatant actor) => actor.Ultimate >= BattleConfig.UltimateMax;

    public Skill ChooseSkill(Combatant actor)
    {
        if (actor.Ultimate >= BattleConfig.UltimateMax)
            return actor.Skills.First(s => s.Kind == SkillKind.Ultimate);

        var art = actor.Skills.FirstOrDefault(s => s.Kind == SkillKind.Attack && s.MpCost > 0 && actor.Mp >= s.MpCost);
        if (art != null) return art;

        return actor.Skills.First(s => s.Kind == SkillKind.Attack && s.MpCost == 0);
    }

    public Combatant ChooseTarget(Combatant actor, List<Combatant> foes)
    {
        Combatant best = null;
        foreach (var f in foes)
            if (f.Alive && (best == null || f.Hp < best.Hp))
                best = f;
        return best!;
    }
}
