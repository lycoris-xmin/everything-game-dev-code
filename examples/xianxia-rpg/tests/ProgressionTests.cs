using NUnit.Framework;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Progression;
using XianxiaRPG.Systems.Loot;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Tests;

[TestFixture]
public class ProgressionTests
{
    static Combatant Enemy(string name, Element el)
    {
        var c = new Combatant { Name = name, IsEnemy = true, Element = el, MaxHp = 60, Hp = 60 };
        c.Attr[AttrKey.Speed] = 25;
        return c;
    }

    [Test]
    public void Crafting_ConsumesMaterialsOnSuccess()
    {
        var inv = new Inventory();
        inv.AddMaterial(new Material { Id = "mat_wind", Name = "风精", Stack = 2 });
        var recipe = new Equipment { Id = "sword", Name = "风灵剑" };
        recipe.Cost["mat_wind"] = 2;
        recipe.Bonus[AttrKey.Attack] = 10;

        Assert.That(CraftingSystem.CanCraft(inv, recipe), Is.True);
        var eq = CraftingSystem.Craft(inv, recipe);
        Assert.That(eq, Is.Not.Null);
        Assert.That(inv.Count("mat_wind"), Is.EqualTo(0));
        Assert.That(inv.Equipments.Count, Is.EqualTo(1));
    }

    [Test]
    public void Crafting_FailsWhenMaterialInsufficient()
    {
        var inv = new Inventory();
        inv.AddMaterial(new Material { Id = "mat_wind", Name = "风精", Stack = 1 });
        var recipe = new Equipment { Id = "sword", Name = "风灵剑" };
        recipe.Cost["mat_wind"] = 2;
        Assert.That(CraftingSystem.CanCraft(inv, recipe), Is.False);
        Assert.That(CraftingSystem.Craft(inv, recipe), Is.Null);
    }

    [Test]
    public void Leveling_GainsAttributesOnLevelUp()
    {
        var lv = new LevelingSystem();
        var attr = new AttributeSet();
        attr[AttrKey.Attack] = 30;
        lv.GainExp(100); // 升到 2 级
        Assert.That(lv.Level, Is.EqualTo(2));
        lv.ApplyGains(attr);
        Assert.That(attr[AttrKey.Attack], Is.EqualTo(32)); // +2*1
    }

    [Test]
    public void DropSystem_ReturnsElementMaterial()
    {
        var drops = DropSystem.RollDrops(Enemy("E", Element.Wind));
        Assert.That(drops.Count, Is.EqualTo(1));
        Assert.That(drops[0].Id, Is.EqualTo("mat_wind"));
    }

    [Test]
    public void CoreLoop_CraftThenEquipMakesStronger()
    {
        var inv = new Inventory();
        var heroAttr = new AttributeSet();
        heroAttr[AttrKey.Attack] = 30;

        // 第一组：掉 1 个风精，差 1 个
        foreach (var m in DropSystem.RollDrops(Enemy("E1", Element.Wind))) inv.AddMaterial(m);
        var recipe = new Equipment { Id = "sword", Name = "风灵剑" };
        recipe.Cost["mat_wind"] = 2;
        recipe.Bonus[AttrKey.Attack] = 10;
        Assert.That(CraftingSystem.CanCraft(inv, recipe), Is.False);

        // 再打一组：补齐材料
        foreach (var m in DropSystem.RollDrops(Enemy("E2", Element.Wind))) inv.AddMaterial(m);
        Assert.That(CraftingSystem.CanCraft(inv, recipe), Is.True);

        var eq = CraftingSystem.Craft(inv, recipe);
        Assert.That(eq, Is.Not.Null);
        Assert.That(inv.Count("mat_wind"), Is.EqualTo(0));

        // 穿戴：面板变强
        heroAttr[AttrKey.Attack] += eq.Bonus[AttrKey.Attack];
        Assert.That(heroAttr[AttrKey.Attack], Is.EqualTo(40));
    }
}
