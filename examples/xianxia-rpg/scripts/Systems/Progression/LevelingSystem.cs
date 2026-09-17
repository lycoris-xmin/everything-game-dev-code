using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Progression;

/// <summary>SYS-003 等级与成长（最小契约）：固定经验曲线，每级线性提升属性。</summary>
public class LevelingSystem
{
    public int Level = 1;
    public int Exp;
    public const int ExpPerLevel = 100;

    public void GainExp(int amount)
    {
        Exp += amount;
        while (Exp >= ExpPerLevel)
        {
            Exp -= ExpPerLevel;
            Level++;
        }
    }

    /// <summary>把当前等级带来的属性增益应用到属性集（每级 +2 攻/灵力，+1 防/灵抗/身法）。</summary>
    public void ApplyGains(AttributeSet attr)
    {
        int bonus = Level - 1;
        attr[AttrKey.Attack] += 2 * bonus;
        attr[AttrKey.MagicPower] += 2 * bonus;
        attr[AttrKey.Defense] += 1 * bonus;
        attr[AttrKey.MagicResist] += 1 * bonus;
        attr[AttrKey.Speed] += 1 * bonus;
    }
}
