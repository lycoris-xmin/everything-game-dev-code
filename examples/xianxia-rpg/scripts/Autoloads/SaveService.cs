using Godot;
using System.Linq;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Persistence;

/// <summary>
/// SYS-005 存档与持久化（M5 实现）。
/// 职责：序列化 ProgressionState + 剧情进度 + 设置到 user://，可选同步 Steam 云存档。
/// 格式：版本化 JSON（便于迁移 / 云同步）。防作弊用轻量签名校验（非强加密，单机）。
/// 开放问题：单 / 多存档位（见 TDD §15），M5 前定 —— 当前实现单存档位。
/// </summary>
[GlobalClass]
public partial class SaveService : Node
{
	public override void _Ready() => GD.Print("[Autoload] SaveService 就绪（M5 接入存档读写）");

	public void Save()
	{
		var p = GetNode<ProgressionState>("/root/ProgressionState");
		var data = new SaveData { RegionId = p.CurrentRegion.Id };
		var hero = p.Party[0];
		var cs = new CharacterSave
		{
			Name = hero.Name,
			Element = hero.Element,
			Hp = hero.Hp,
			MaxHp = hero.MaxHp,
			Mp = hero.Mp,
			MaxMp = hero.MaxMp,
			Level = p.Level.Level,
			Exp = p.Level.Exp
		};
		for (int i = 0; i < AttributeSet.Count; i++) cs.Attr[i] = hero.Attr[(AttrKey)i];
		data.Characters.Add(cs);

		foreach (var m in p.Inv.Materials)
			data.Inventory.Materials.Add(new MaterialSave { Id = m.Id, Name = m.Name, Stack = m.Stack });
		foreach (var e in p.Inv.Equipments)
		{
			var es = new EquipmentSave { Id = e.Id, Name = e.Name };
			for (int i = 0; i < AttributeSet.Count; i++) es.Bonus[i] = e.Bonus[(AttrKey)i];
			data.Inventory.Equipments.Add(es);
		}
		foreach (var s in p.ClearedSpots) data.ClearedSpots.Add(s);

		string json = SaveSystem.ToJson(data);
		using var f = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Write);
		f.StoreString(json);
		GD.Print("[Save] 已写入 user://save.json");
	}

	public bool Load()
	{
		if (!FileAccess.FileExists("user://save.json")) return false;
		using var f = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Read);
		var data = SaveSystem.FromJson(f.GetAsText());
		var p = GetNode<ProgressionState>("/root/ProgressionState");

		p.CurrentRegion = p.BuildRegion();
		p.Party.Clear();
		foreach (var cs in data.Characters)
		{
			var c = new Combatant { Name = cs.Name, Element = cs.Element, MaxHp = cs.MaxHp, Hp = cs.Hp, MaxMp = cs.MaxMp, Mp = cs.Mp, MaxToughness = 100, Toughness = 100 };
			for (int i = 0; i < AttributeSet.Count; i++) c.Attr[(AttrKey)i] = cs.Attr[i];
			p.Party.Add(c);
		}
		p.Level.Level = data.Characters.FirstOrDefault()?.Level ?? 1;
		p.Level.Exp = data.Characters.FirstOrDefault()?.Exp ?? 0;
		p.Inv.Materials.Clear();
		foreach (var m in data.Inventory.Materials)
			p.Inv.Materials.Add(new XianxiaRPG.Systems.Crafting.Material { Id = m.Id, Name = m.Name, Stack = m.Stack });
		p.Inv.Equipments.Clear();
		foreach (var e in data.Inventory.Equipments)
		{
			var eq = new Equipment { Id = e.Id, Name = e.Name };
			for (int i = 0; i < AttributeSet.Count; i++) eq.Bonus[(AttrKey)i] = e.Bonus[i];
			p.Inv.Equipments.Add(eq);
		}
		p.ClearedSpots = new System.Collections.Generic.HashSet<int>(data.ClearedSpots);
		GD.Print("[Save] 已读档");
		return true;
	}
}
