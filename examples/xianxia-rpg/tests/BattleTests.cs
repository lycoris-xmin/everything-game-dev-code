using NUnit.Framework;
using System.Collections.Generic;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Battle;

namespace XianxiaRPG.Tests;

[TestFixture]
public class BattleTests
{
    static Combatant Hero(string name, int speed, Element el, int hp = 100, int atk = 30, int mpow = 30, int def = 10, int mres = 10, int ult = 0)
    {
        var c = new Combatant { Name = name, IsEnemy = false, Element = el, MaxHp = hp, Hp = hp, MaxMp = 50, Mp = 50, MaxToughness = 100, Toughness = 100, Ultimate = ult };
        c.Attr[AttrKey.Attack] = atk; c.Attr[AttrKey.MagicPower] = mpow;
        c.Attr[AttrKey.Defense] = def; c.Attr[AttrKey.MagicResist] = mres; c.Attr[AttrKey.Speed] = speed;
        c.AddSkill(new Skill { Id = "atk", Name = "普攻", Element = el, Power = 10, MpCost = 0, Kind = SkillKind.Attack });
        c.AddSkill(new Skill { Id = "art", Name = "仙术", Element = el, Power = 25, MpCost = 15, Kind = SkillKind.Attack });
        c.AddSkill(new Skill { Id = "ult", Name = "终结技", Element = el, Power = 50, MpCost = 0, Kind = SkillKind.Ultimate });
        return c;
    }

    static Combatant Enemy(string name, int speed, Element el, int hp = 80, int atk = 20, int mpow = 20, int def = 8, int mres = 8, int tough = 100)
    {
        var c = Hero(name, speed, el, hp, atk, mpow, def, mres);
        c.IsEnemy = true; c.MaxToughness = tough; c.Toughness = tough;
        return c;
    }

    static SimpleAI AI() => new();

    [Test]
    public void AvQueue_HigherSpeedActsFirst()
    {
        var fast = Hero("Fast", 80, Element.Fire);
        var slow = Hero("Slow", 30, Element.Water);
        var foe = Enemy("E", 20, Element.Earth, hp: 500);
        var log = BattleEngine.Run(new List<Combatant> { fast, slow }, new List<Combatant> { foe }, AI());

        var first = log.Events.Find(e => e.Action == "skill" || e.Action == "ultimate");
        Assert.That(first, Is.Not.Null);
        Assert.That(first!.Actor, Is.EqualTo("Fast"));
    }

    [Test]
    public void ToughnessBreak_TriggersOnCounterElement()
    {
        var p = Hero("P", 80, Element.Wind); // 风克雷
        var e = Enemy("E", 20, Element.Thunder, hp: 100, tough: 30);
        var log = BattleEngine.Run(new List<Combatant> { p }, new List<Combatant> { e }, AI());

        Assert.That(log.Events.Exists(ev => ev.Action == "broken"), Is.True);
    }

    [Test]
    public void Ultimate_InsertsWhenReady()
    {
        var p = Hero("P", 50, Element.Fire, ult: 100); // 初始满槽，应插队
        var e = Enemy("E", 20, Element.Earth, hp: 500);
        var log = BattleEngine.Run(new List<Combatant> { p }, new List<Combatant> { e }, AI());

        Assert.That(log.Events[0].Action, Is.EqualTo("ultimate"));
    }

    [Test]
    public void FullBattle_FourVsTwo_PlayerWins()
    {
        var allies = new List<Combatant>
        {
            Hero("A", 60, Element.Wind),
            Hero("B", 55, Element.Thunder),
            Hero("C", 50, Element.Water),
            Hero("D", 45, Element.Fire),
        };
        var foes = new List<Combatant>
        {
            Enemy("E1", 30, Element.Earth, hp: 60),
            Enemy("E2", 25, Element.Earth, hp: 60),
        };
        var log = BattleEngine.Run(allies, foes, AI());

        Assert.That(log.Outcome, Is.EqualTo("player_win"));
        Assert.That(log.Events.Exists(ev => ev.Action == "death" && ev.Actor == "E1"), Is.True);
        Assert.That(log.Events.Exists(ev => ev.Action == "death" && ev.Actor == "E2"), Is.True);
        Assert.That(log.TotalTicks, Is.LessThan(BattleConfig.MaxTicks));
    }

    [Test]
    public void Log_IsReplayable()
    {
        var allies = new List<Combatant> { Hero("A", 60, Element.Wind) };
        var foes = new List<Combatant> { Enemy("E1", 30, Element.Earth, hp: 40) };
        var log = BattleEngine.Run(allies, foes, AI());

        Assert.That(log.Outcome, Is.EqualTo("player_win"));
        var replay = string.Join("|", log.Events.ConvertAll(e => $"{e.Tick}:{e.Actor}:{e.Action}"));
        Assert.That(replay, Does.Contain("E1:death"));
    }
}
