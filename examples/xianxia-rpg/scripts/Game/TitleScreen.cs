using Godot;
using XianxiaRPG.UI;

/// <summary>
/// 标题 / 开始菜单（仙侠风）：山水背景 + 暗化遮罩 + 居中标题与开始按钮。
/// 点击「开始游戏」进入世界。
/// </summary>
[GlobalClass]
public partial class TitleScreen : Control
{
    public Main Root;

    public override void _Ready()
    {
        AnchorRight = 1; AnchorBottom = 1;

        var bg = new TextureRect
        {
            Texture = UiKit.LoadTex("bg_world.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        var veil = new ColorRect { Color = new Color(0.04f, 0.08f, 0.12f, 0.55f) };
        veil.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(veil);

        var vb = new VBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        vb.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        vb.AddThemeConstantOverride("separation", 26);
        AddChild(vb);

        var title = UiKit.MakeLabel("仙侠 · 回合", UiKit.Gold, 64);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(title);

        var sub = UiKit.MakeLabel("青云山脚 · 风灵剑修 云无月", UiKit.TextDim, 22);
        sub.HorizontalAlignment = HorizontalAlignment.Center;
        sub.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(sub);

        var start = UiKit.MakeButton("开始游戏", UiKit.LoadTex("hero.png"), UiKit.Gold);
        start.CustomMinimumSize = new Vector2(300, 56);
        start.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        start.Pressed += () => Root.EnterWorld();
        vb.AddChild(start);

        var tip = UiKit.MakeLabel("WASD 移动 · 靠近红点按空格遭遇 · Tab 角色/打造 · 战斗中点击技能", UiKit.TextDim, 16);
        tip.HorizontalAlignment = HorizontalAlignment.Center;
        tip.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(tip);
    }
}
