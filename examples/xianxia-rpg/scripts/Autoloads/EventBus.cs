using Godot;

/// <summary>
/// 全局事件总线（rules/godot/signals-events.md）。
/// 战斗 / 经济关键动作经此处广播给 UI / Telemetry，保持解耦、所有权清晰。
/// 信号命名与载荷需明确（见下方 Signal 定义）。
/// </summary>
[GlobalClass]
public partial class EventBus : Node
{
	/// <summary>战斗结束。victory=true 表示胜利。</summary>
	[Signal]
	public delegate void BattleEndedEventHandler(bool victory);

	/// <summary>完成一次打造。itemId 为装备/物品配置 id。</summary>
	[Signal]
	public delegate void ItemCraftedEventHandler(string itemId);

	/// <summary>获取材料。materialId + 数量，供 Telemetry / 经济校验。</summary>
	[Signal]
	public delegate void MaterialGainedEventHandler(string materialId, int amount);

	public override void _Ready()
	{
		GD.Print("[Autoload] EventBus 就绪");
	}
}
