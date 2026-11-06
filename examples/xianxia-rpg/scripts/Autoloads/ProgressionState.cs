using Godot;
using System.Collections.Generic;
using XianxiaRPG.Systems.Battle;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Exploration;
using XianxiaRPG.Systems.Progression;

/// <summary>
/// 运行时进度态（等级 / 装备 / 材料 / 剧情标志）— 单机唯一真相源。
/// 存档（SaveService）为其持久化镜像。战斗结束后回收的临时态不在此。
/// </summary>
[GlobalClass]
public partial class ProgressionState : Node
{
	public List<Combatant> Party = new();
	public Inventory Inv = new();
	public LevelingSystem Level = new();
	public Region CurrentRegion;
	public HashSet<int> ClearedSpots = new();
	public List<Equipment> Recipes = new();

	public override void _Ready()
	{
		InitDefault();
		GD.Print("[Autoload] ProgressionState 就绪");
	}

	void InitDefault()
	{
		Party.Clear();
		Inv = new Inventory();
		Level = new LevelingSystem();
		ClearedSpots.Clear();
		Recipes.Clear();
		var hero = new Combatant { Name = "云无月", Element = Element.Wind, MaxHp = 200, Hp = 200, MaxMp = 100, Mp = 100, MaxToughness = 100, Toughness = 100 };
		hero.Attr[AttrKey.Attack] = 30;
		hero.Attr[AttrKey.MagicPower] = 25;
		hero.Attr[AttrKey.Defense] = 20;
		hero.Attr[AttrKey.MagicResist] = 18;
		hero.Attr[AttrKey.Speed] = 28;
		hero.AddSkill(new Skill { Id = "atk", Name = "普攻", Description = "挥剑直击敌方弱点", Kind = SkillKind.Attack, MpCost = 0, Power = 20, Element = Element.Wind });
		hero.AddSkill(new Skill { Id = "art", Name = "御风术", Description = "借风灵之力造成多段伤害", Kind = SkillKind.Attack, MpCost = 20, Power = 36, Element = Element.Wind });
		hero.AddSkill(new Skill { Id = "ult", Name = "风云斩", Description = "凝聚风云之力，破韧终结一击", Kind = SkillKind.Ultimate, MpCost = 0, Power = 50, Element = Element.Wind });
		Party.Add(hero);

		Level = new LevelingSystem();
		CurrentRegion = BuildRegion();

		var sword = new Equipment { Id = "sword", Name = "风灵剑" };
		sword.Cost["mat_wind"] = 2;
		sword.Bonus[AttrKey.Attack] = 10;
		Recipes.Add(sword);
	}

	/// <summary>重新开始：重置为默认进度（覆盖读档态）。</summary>
	public void ResetProgress() => InitDefault();

	/// <summary>构造默认区域（含遭遇点与敌人）。读档后用它重建区域定义，再叠加已清进度。</summary>
	public Region BuildRegion()
	{
		var r = new Region { Id = "r1", Name = "青云山脚" };
		var s0 = new EncounterSpot { Id = 0, Name = "山道" };
		s0.Enemies.Add(MakeEnemy("野狼", Element.Wind, 70, 18));
		var s1 = new EncounterSpot { Id = 1, Name = "林间" };
		s1.Enemies.Add(MakeEnemy("妖藤", Element.Thunder, 90, 22));
		s1.Enemies.Add(MakeEnemy("毒蛛", Element.Water, 60, 16));
		r.Spots.Add(s0);
		r.Spots.Add(s1);
		return r;
	}

	public static Combatant MakeEnemy(string name, Element el, int hp, int atk)
	{
		var c = new Combatant { Name = name, IsEnemy = true, Element = el, MaxHp = hp, Hp = hp, MaxToughness = 80, Toughness = 80 };
		c.Attr[AttrKey.Attack] = atk;
		c.Attr[AttrKey.Speed] = 22;
		c.AddSkill(new Skill { Id = "atk", Name = "撕咬", Kind = SkillKind.Attack, MpCost = 0, Power = (int)(atk * 0.8) });
		c.AddSkill(new Skill { Id = "ult", Name = "猛扑", Kind = SkillKind.Ultimate, MpCost = 0, Power = (int)(atk * 1.5) });
		return c;
	}
}
