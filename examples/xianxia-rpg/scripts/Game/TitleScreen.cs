using Godot;
using XianxiaRPG.UI;

/// <summary>
/// 标题 / 开始菜单（仙侠水墨风）：水墨山峦背景 + 暗化遮罩 + 居中标题、分隔与开始按钮。
/// 点击「开始游戏 / 继续游戏」进入世界；有存档时额外提供「重新开始」。
/// </summary>
[GlobalClass]
public partial class TitleScreen : Control
{
    public Main Root;
    public bool HasSave;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        UiKit.StackBg(this, "pixel/bg_sky.png", "pixel/bg_far.png", "pixel/bg_mid.png", "pixel/bg_near.png");

        var veil = new ColorRect { Color = new Color(0.03f, 0.06f, 0.1f, 0.42f) };
        veil.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        veil.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(veil);

        // 八卦水印（淡淡居中，烘托国风，置于内容之下）
        var bagua = new TextureRect
        {
            Texture = UiKit.LoadTex("pixel/motif_bagua.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(420, 420),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            Modulate = new Color(1, 1, 1, 0.06f),
        };
        bagua.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        bagua.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(bagua);

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        center.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(center);

        var frame = new Panel();
        frame.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgDeep, UiKit.Gold));
        frame.MouseFilter = MouseFilterEnum.Ignore;
        center.AddChild(frame);

        var vb = new VBoxContainer();
        vb.AddThemeConstantOverride("separation", 22);
        vb.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vb.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        frame.AddChild(vb);

        var title = UiKit.MakeLabel("仙 侠 · 回 合 制", UiKit.Gold, 76);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(title);

        var sub = UiKit.MakeLabel("青云山脚 · 风灵剑修 云无月", UiKit.TextDim, 22);
        sub.HorizontalAlignment = HorizontalAlignment.Center;
        sub.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(sub);

        // 祥云分隔
        var cloud = new TextureRect
        {
            Texture = UiKit.LoadTex("pixel/motif_cloud.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(160, 40),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        vb.AddChild(cloud);

        var start = UiKit.MakeButtonPixel(HasSave ? "继续游戏" : "开始游戏", null, UiKit.Gold);
        start.CustomMinimumSize = new Vector2(320, 60);
        start.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        start.Pressed += OnStart;
        vb.AddChild(start);
        start.GrabFocus();

        if (HasSave)
        {
            var ng = UiKit.MakeButtonPixel("重新开始", null, UiKit.Jade);
            ng.CustomMinimumSize = new Vector2(320, 48);
            ng.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            ng.Pressed += () =>
            {
                Root.Prog.ResetProgress();
                Root.GetNode<SaveService>("/root/SaveService").DeleteSave();
                Root.EnterWorld();
            };
            vb.AddChild(ng);
        }

        var tip = UiKit.MakeLabel("WASD 移动 · 靠近红色精灵按空格遭遇 · Tab 角色/打造 · 战斗中点技能出招", UiKit.TextDim, 16);
        tip.HorizontalAlignment = HorizontalAlignment.Center;
        tip.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(tip);

        // 朱印（右下角钤印）
        var seal = new TextureRect
        {
            Texture = UiKit.LoadTex("pixel/motif_seal.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(36, 36),
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            SizeFlagsVertical = SizeFlags.ShrinkEnd,
        };
        vb.AddChild(seal);

        // 入场淡入
        Modulate = new Color(1, 1, 1, 0);
        var tw = CreateTween();
        tw.TweenProperty(this, "modulate:a", 1f, 0.5);
    }

    void OnStart() => Root?.EnterWorld();
}
