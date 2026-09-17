using Godot;

/// <summary>
/// 遥测（GDD §11.3）：打造次数、材料获取 / 消耗、战斗胜率。
/// 订阅 EventBus；离线时仅本地日志，上线后接 Steam / 分析后端。
/// </summary>
[GlobalClass]
public partial class Telemetry : Node
{
	public override void _Ready()
	{
		GD.Print("[Autoload] Telemetry 就绪（离线模式：本地日志）");
	}
}
