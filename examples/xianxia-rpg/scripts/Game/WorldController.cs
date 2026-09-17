using Godot;
using XianxiaRPG.Systems.Exploration;
using XianxiaRPG.UI;

/// <summary>
/// M3 区域场景（仙侠风）：青绿山水背景 + 主角立绘 + 遭遇点触发 + WASD 移动。
/// 背景与主角立绘来自 res://assets 下生成的位图；遭遇点用红/灰圆标记。
/// </summary>
[GlobalClass]
public partial class WorldController : Node2D
{
    public Main Root;
    private Vector2 _player = new(200, 540);
    private const float Speed = 320f;
    private PortraitView _avatar;

    public override void _Ready()
    {
        var bg = new TextureRect
        {
            Texture = UiKit.LoadTex("bg_world.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        _avatar = new PortraitView
        {
            Tex = UiKit.LoadTex("hero.png"),
            Title = "云无月",
            Robe = UiKit.Jade,
            Size = new Vector2(90, 90),
            Position = _player - new Vector2(45, 45),
        };
        AddChild(_avatar);
    }

    public override void _Process(double delta)
    {
        var v = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W)) v.Y -= 1;
        if (Input.IsKeyPressed(Key.S)) v.Y += 1;
        if (Input.IsKeyPressed(Key.A)) v.X -= 1;
        if (Input.IsKeyPressed(Key.D)) v.X += 1;
        if (v != Vector2.Zero) _player += v.Normalized() * Speed * (float)delta;
        _player = new Vector2(Mathf.Clamp(_player.X, 40, 1880), Mathf.Clamp(_player.Y, 40, 1040));
        _avatar.Position = _player - new Vector2(45, 45);

        if (Input.IsKeyPressed(Key.Tab)) { Root.EnterParty(); return; }

        var region = Root.Prog.CurrentRegion;
        for (int i = 0; i < region.Spots.Count; i++)
        {
            var spot = region.Spots[i];
            if (Root.Prog.ClearedSpots.Contains(spot.Id)) continue;
            if (_player.DistanceTo(SpotPos(i)) < 70 && Input.IsKeyPressed(Key.Space))
            {
                Root.EnterBattle(spot.Id);
                return;
            }
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        var region = Root.Prog.CurrentRegion;
        for (int i = 0; i < region.Spots.Count; i++)
        {
            var spot = region.Spots[i];
            bool cleared = Root.Prog.ClearedSpots.Contains(spot.Id);
            DrawCircle(SpotPos(i), 34, cleared ? UiKit.TextDim : UiKit.Crimson);
            var font = ThemeDB.FallbackFont;
            if (font != null)
                DrawString(font, SpotPos(i) - new Vector2(24, 70), $"{spot.Id}:{spot.Name}", HorizontalAlignment.Left, 0, 18, UiKit.TextCol);
        }
        var f = ThemeDB.FallbackFont;
        if (f != null)
            DrawString(f, new Vector2(20, 30),
                $"区域：{region.Name}   WASD 移动 · 靠近红点按空格遭遇 · Tab 开角色/打造面板",
                HorizontalAlignment.Left, 0, 18, UiKit.Gold);
    }

    private Vector2 SpotPos(int i) => new(500 + i * 450, 540);
}
