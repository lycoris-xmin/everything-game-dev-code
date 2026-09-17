using System.Collections.Generic;
using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Persistence;

/// <summary>SYS-005 存档 DTO（最小契约：单存档位，仅存可恢复的最小字段）。</summary>
public class CharacterSave
{
    public string Name = "";
    public Element Element;
    public int[] Attr = new int[AttributeSet.Count];
    public int Hp;
    public int MaxHp;
    public int Mp;
    public int MaxMp;
    public int Level;
    public int Exp;
}

public class MaterialSave
{
    public string Id = "";
    public string Name = "";
    public int Stack;
}

public class EquipmentSave
{
    public string Id = "";
    public string Name = "";
    public int[] Bonus = new int[AttributeSet.Count];
}

public class InventorySave
{
    public List<MaterialSave> Materials = new();
    public List<EquipmentSave> Equipments = new();
}

public class SaveData
{
    public List<CharacterSave> Characters = new();
    public InventorySave Inventory = new();
    public string RegionId = "";
    public List<int> ClearedSpots = new();
}
