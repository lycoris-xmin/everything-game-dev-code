using System.Collections.Generic;

namespace XianxiaRPG.Systems.Core;

/// <summary>
/// SYS-002 五灵元素。纯枚举，无引擎依赖。
/// </summary>
public enum Element
{
    Wind,    // 风
    Thunder, // 雷
    Water,   // 水
    Fire,    // 火
    Earth    // 土
}

/// <summary>
/// SYS-002 五灵相克/相生环。纯函数，无副作用（design/gdd/attribute.md §2）。
/// 相克：风→雷→水→火→土→风（攻击方克守方 ×1.3）
/// 相生：风→火→雷→土→水→风（攻击方生守方，守方受生减伤 ×0.85）
/// </summary>
public static class ElementRing
{
    public const float CounterMultiplier = 1.3f;
    public const float BoostMultiplier = 0.85f;
    public const float NeutralMultiplier = 1.0f;

    private static readonly Dictionary<Element, Element> Counter = new()
    {
        { Element.Wind, Element.Thunder },
        { Element.Thunder, Element.Water },
        { Element.Water, Element.Fire },
        { Element.Fire, Element.Earth },
        { Element.Earth, Element.Wind },
    };

    private static readonly Dictionary<Element, Element> Boost = new()
    {
        { Element.Wind, Element.Fire },
        { Element.Fire, Element.Thunder },
        { Element.Thunder, Element.Earth },
        { Element.Earth, Element.Water },
        { Element.Water, Element.Wind },
    };

    /// <summary>
    /// 攻击方对守方的元素倍率。
    /// 同元素=1.0；相克=1.3；相生=0.85；无关=1.0。
    /// </summary>
    public static float GetMultiplier(Element atk, Element def)
    {
        if (atk == def) return NeutralMultiplier;
        if (Counter.TryGetValue(atk, out var countered) && countered == def) return CounterMultiplier;
        if (Boost.TryGetValue(atk, out var boosted) && boosted == def) return BoostMultiplier;
        return NeutralMultiplier;
    }

    /// <summary>atk 是否相克 def（用于韧性削减判定，避免浮点比较）。</summary>
    public static bool IsCounter(Element atk, Element def)
        => Counter.TryGetValue(atk, out var countered) && countered == def;
}
