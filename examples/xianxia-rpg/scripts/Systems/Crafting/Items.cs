using System.Collections.Generic;
using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Crafting;

/// <summary>SYS-012 材料。</summary>
public class Material
{
    public string Id = "";
    public string Name = "";
    public int Stack;
}

/// <summary>SYS-011 装备（最小契约：线性成本、100% 成功、属性加成）。</summary>
public class Equipment
{
    public string Id = "";
    public string Name = "";
    public AttributeSet Bonus = new();
    public Dictionary<string, int> Cost = new(); // materialId -> 数量
}

/// <summary>背包：材料堆叠 + 装备列表。</summary>
public class Inventory
{
    public List<Material> Materials = new();
    public List<Equipment> Equipments = new();

    public void AddMaterial(Material m)
    {
        var ex = Materials.Find(x => x.Id == m.Id);
        if (ex != null) ex.Stack += m.Stack;
        else Materials.Add(new Material { Id = m.Id, Name = m.Name, Stack = m.Stack });
    }

    public int Count(string id) => Materials.Find(x => x.Id == id)?.Stack ?? 0;

    public bool Consume(string id, int n)
    {
        var ex = Materials.Find(x => x.Id == id);
        if (ex == null || ex.Stack < n) return false;
        ex.Stack -= n;
        if (ex.Stack == 0) Materials.Remove(ex);
        return true;
    }
}
