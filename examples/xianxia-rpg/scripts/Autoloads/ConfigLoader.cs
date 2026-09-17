using Godot;

/// <summary>
/// SYS-004 数据配置系统（M1 实现）。
/// 职责：加载敌人 / 技能 / 装备 / 材料 / 配方 / 区域等配置表。
/// 建议：数值密集型用 CSV/JSON（策划友好），引用型用 Resource。
/// </summary>
[GlobalClass]
public partial class ConfigLoader : Node
{
	public override void _Ready()
	{
		GD.Print("[Autoload] ConfigLoader 就绪（M1 接入配置表）");
	}
}
