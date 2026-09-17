using Godot;

/// <summary>
/// 集中输入抽象（rules/godot/ui-input.md）。
/// 游戏逻辑只响应意图级 action（battle_select / world_interact / menu_confirm ...），
/// 不硬编码设备；支持手柄 / 键鼠 / 重映射。M5 前绑定键位并验证手柄导航。
/// </summary>
[GlobalClass]
public partial class InputManager : Node
{
	public override void _Ready()
	{
		GD.Print("[Autoload] InputManager 就绪");
	}
}
