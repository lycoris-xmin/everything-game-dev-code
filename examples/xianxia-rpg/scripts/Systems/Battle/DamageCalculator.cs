using System;
using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Battle;

/// <summary>
/// SYS-006（最小契约）伤害结算。纯函数、确定性、无随机（design/gdd/attribute.md §3）。
/// 随机/暴击在 M2 接入；M1 用固定 SkillPower 做确定性断言。
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// 计算一次伤害。
    /// offense = Attack + MagicPower + skillPower
    /// defense = Defense + MagicResist
    /// damage  = max(1, round((offense - defense) * ElementRing.GetMultiplier(atkEl, defEl)))
    /// </summary>
    public static int ComputeDamage(AttributeSet attacker, Element attackerElement,
                                    AttributeSet defender, Element defenderElement,
                                    int skillPower)
    {
        int offense = attacker[AttrKey.Attack] + attacker[AttrKey.MagicPower] + skillPower;
        int defense = defender[AttrKey.Defense] + defender[AttrKey.MagicResist];
        int raw = offense - defense;
        float mult = ElementRing.GetMultiplier(attackerElement, defenderElement);
        int damage = (int)Math.Round(raw * mult);
        return Math.Max(1, damage);
    }
}
