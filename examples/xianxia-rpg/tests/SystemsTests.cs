using NUnit.Framework;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Battle;

/// <summary>
/// M1 确定性测试（PLAN §2 退出标准）。
/// 覆盖属性加法、五灵倍率、伤害结算。
/// </summary>
[TestFixture]
public class SystemsTests
{
    [Test]
    public void AttributeSet_AddBonus_IsAdditive()
    {
        var a = new AttributeSet();
        a[AttrKey.Attack] = 100;
        a.AddBonus(AttrKey.Attack, 30);
        Assert.That(a[AttrKey.Attack], Is.EqualTo(130));
        Assert.That(a.ComputeFinal(AttrKey.Attack), Is.EqualTo(130));
    }

    [Test]
    public void ElementRing_Counter_ReturnsCounterMultiplier()
    {
        Assert.That(ElementRing.GetMultiplier(Element.Wind, Element.Thunder), Is.EqualTo(ElementRing.CounterMultiplier));
        Assert.That(ElementRing.GetMultiplier(Element.Fire, Element.Earth), Is.EqualTo(ElementRing.CounterMultiplier));
    }

    [Test]
    public void ElementRing_Boost_ReturnsBoostMultiplier()
    {
        Assert.That(ElementRing.GetMultiplier(Element.Wind, Element.Fire), Is.EqualTo(ElementRing.BoostMultiplier));
        Assert.That(ElementRing.GetMultiplier(Element.Earth, Element.Water), Is.EqualTo(ElementRing.BoostMultiplier));
    }

    [Test]
    public void ElementRing_SameElement_IsNeutral()
    {
        Assert.That(ElementRing.GetMultiplier(Element.Fire, Element.Fire), Is.EqualTo(ElementRing.NeutralMultiplier));
    }

    [Test]
    public void ElementRing_Unrelated_IsNeutral()
    {
        Assert.That(ElementRing.GetMultiplier(Element.Wind, Element.Earth), Is.EqualTo(ElementRing.NeutralMultiplier));
    }

    [Test]
    public void DamageCalculator_IsDeterministic_Neutral()
    {
        var atk = new AttributeSet();
        atk[AttrKey.Attack] = 120; atk[AttrKey.MagicPower] = 100;
        var def = new AttributeSet();
        def[AttrKey.Defense] = 80; def[AttrKey.MagicResist] = 70;
        // offense=220, defense=150, raw=70, Fire vs Wind 无关=1.0 -> 70
        int dmg = DamageCalculator.ComputeDamage(atk, Element.Fire, def, Element.Wind, 0);
        Assert.That(dmg, Is.EqualTo(70));
    }

    [Test]
    public void DamageCalculator_CounterAmplifies()
    {
        var atk = new AttributeSet();
        atk[AttrKey.Attack] = 120; atk[AttrKey.MagicPower] = 100;
        var def = new AttributeSet();
        def[AttrKey.Defense] = 80; def[AttrKey.MagicResist] = 70;
        // Fire 克 Earth -> ×1.3, raw=70 -> round(91)=91
        int dmg = DamageCalculator.ComputeDamage(atk, Element.Fire, def, Element.Earth, 0);
        Assert.That(dmg, Is.EqualTo(91));
    }

    [Test]
    public void DamageCalculator_FloorIsOne()
    {
        var atk = new AttributeSet();
        atk[AttrKey.Attack] = 10;
        var def = new AttributeSet();
        def[AttrKey.Defense] = 200; def[AttrKey.MagicResist] = 200;
        int dmg = DamageCalculator.ComputeDamage(atk, Element.Wind, def, Element.Wind, 0);
        Assert.That(dmg, Is.EqualTo(1));
    }
}
