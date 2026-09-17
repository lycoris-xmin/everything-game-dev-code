using NUnit.Framework;
using System.Collections.Generic;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Battle;
using XianxiaRPG.Systems.Exploration;

namespace XianxiaRPG.Tests;

[TestFixture]
public class ExplorationTests
{
    static Combatant Enemy(string name, int speed, Element el, int hp = 60)
    {
        var c = new Combatant { Name = name, IsEnemy = true, Element = el, MaxHp = hp, Hp = hp, MaxMp = 30, Mp = 30, MaxToughness = 80, Toughness = 80 };
        c.Attr[AttrKey.Attack] = 20; c.Attr[AttrKey.MagicPower] = 20;
        c.Attr[AttrKey.Defense] = 8; c.Attr[AttrKey.MagicResist] = 8; c.Attr[AttrKey.Speed] = speed;
        c.AddSkill(new Skill { Id = "atk", Name = "普攻", Element = el, Power = 10, MpCost = 0, Kind = SkillKind.Attack });
        c.AddSkill(new Skill { Id = "ult", Name = "终结技", Element = el, Power = 50, MpCost = 0, Kind = SkillKind.Ultimate });
        return c;
    }

    static Region BuildRegion()
    {
        var r = new Region { Id = "r1", Name = "青云麓" };
        for (int i = 0; i < 3; i++)
            r.Spots.Add(new EncounterSpot
            {
                Id = i,
                Name = $"遭遇{i}",
                Enemies = new List<Combatant> { Enemy($"E{i}a", 30, Element.Earth), Enemy($"E{i}b", 25, Element.Earth) }
            });
        r.EndNode = new StoryNode { Id = "end", Text = "你走出了青云麓。" };
        return r;
    }

    [Test]
    public void Region_TriggersThreeEncountersThenStoryNode()
    {
        var sys = new EncounterSystem(BuildRegion());
        int fought = 0;
        while (!sys.IsComplete)
        {
            var enemies = sys.StartNextEncounter();
            Assert.That(enemies, Is.Not.Null);
            Assert.That(enemies.Count, Is.EqualTo(2));
            fought++;
            sys.MarkCleared();
        }
        Assert.That(fought, Is.EqualTo(3));
        Assert.That(sys.ReachedEndNode(), Is.Not.Null);
        Assert.That(sys.ReachedEndNode()!.Text, Does.Contain("青云麓"));
    }

    [Test]
    public void EncounterSystem_ProgressIsSequential()
    {
        var sys = new EncounterSystem(BuildRegion());
        Assert.That(sys.CurrentSpot()!.Id, Is.EqualTo(0));
        sys.MarkCleared();
        Assert.That(sys.CurrentSpot()!.Id, Is.EqualTo(1));
        sys.MarkCleared();
        Assert.That(sys.CurrentSpot()!.Id, Is.EqualTo(2));
    }
}
