using System.Collections.Generic;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Systems.Loot;

/// <summary>SYS-014 掉落（最小契约）：确定性掉落，按敌人元素给固定材料。</summary>
public static class DropSystem
{
    public static List<Material> RollDrops(Combatant enemy)
    {
        return new List<Material>
        {
            new Material { Id = "mat_" + enemy.Element.ToString().ToLowerInvariant(), Name = enemy.Element + "精", Stack = 1 }
        };
    }
}
