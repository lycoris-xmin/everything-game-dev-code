using System.Collections.Generic;

namespace XianxiaRPG.Systems.Crafting;

/// <summary>SYS-031 打造（最小契约）：材料足够则 100% 成功，消耗材料产出装备。</summary>
public static class CraftingSystem
{
    public static bool CanCraft(Inventory inv, Equipment recipe)
    {
        foreach (var kv in recipe.Cost)
            if (inv.Count(kv.Key) < kv.Value) return false;
        return true;
    }

    public static Equipment Craft(Inventory inv, Equipment recipe)
    {
        if (!CanCraft(inv, recipe)) return null;
        foreach (var kv in recipe.Cost) inv.Consume(kv.Key, kv.Value);
        var made = new Equipment { Id = recipe.Id, Name = recipe.Name };
        made.Bonus = recipe.Bonus.Clone();
        made.Cost = new Dictionary<string, int>(recipe.Cost);
        inv.Equipments.Add(made);
        return made;
    }
}
