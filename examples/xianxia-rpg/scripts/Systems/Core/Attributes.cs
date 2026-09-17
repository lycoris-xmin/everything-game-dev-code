using System;

namespace XianxiaRPG.Systems.Core;

/// <summary>
/// SYS-001 属性系统。10 项面板数值，全加法、无乘法项（GDD §5.2 / design/gdd/attribute.md）。
/// 纯 C#，无引擎依赖，可直接 NUnit 测试。
/// </summary>
public enum AttrKey
{
    Attack,      // 攻击
    Defense,     // 防御
    MagicPower,  // 灵力（五灵攻击）
    MagicResist, // 灵抗
    Speed,       // 身法（AV 行动值）
    CritRate,    // 暴击率(%)
    Toughness,   // 韧性（击破抗性）
    Accuracy,    // 命中
    Evasion,     // 闪避
    Luck         // 幸运
}

public class AttributeSet
{
    public const int Count = 10;
    private readonly int[] _base = new int[Count];
    private readonly int[] _bonus = new int[Count];

    /// <summary>最终值 = Base + Σ Bonus（全加法）。</summary>
    public int this[AttrKey key]
    {
        get => _base[(int)key] + _bonus[(int)key];
        set => _base[(int)key] = value;
    }

    /// <summary>叠加一项增益/减益（可负）。</summary>
    public void AddBonus(AttrKey key, int value) => _bonus[(int)key] += value;

    /// <summary>显式最终值计算，等价于索引器；保留以匹配设计文档命名。</summary>
    public int ComputeFinal(AttrKey key) => _base[(int)key] + _bonus[(int)key];

    public AttributeSet Clone()
    {
        var copy = new AttributeSet();
        Array.Copy(_base, copy._base, Count);
        Array.Copy(_bonus, copy._bonus, Count);
        return copy;
    }
}
