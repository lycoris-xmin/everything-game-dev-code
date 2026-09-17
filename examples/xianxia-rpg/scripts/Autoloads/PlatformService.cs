using Godot;

/// <summary>
/// 平台集成层（Steam，M6+ 接入）。
/// 通过 IPlatform 接口隔离实现；离线用空实现以便开发，上线接入 GodotSteam(C#) / Steamworks.NET。
/// 集成点：SaveService(云存档)、Telemetry(成就事件)。功能范围待定（见 TDD §15）。
/// </summary>
[GlobalClass]
public partial class PlatformService : Node
{
	public override void _Ready()
	{
		GD.Print("[Autoload] PlatformService 就绪（离线空实现）");
	}
}
