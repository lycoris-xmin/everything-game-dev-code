using Godot;

namespace XianxiaRPG.UI;

/// <summary>数值比例条（程序化绘制）：底槽 + 彩色填充，按 Ratio 0..1 显示。跨版本稳定。</summary>
public partial class BarView : Control
{
    public float Ratio;
    public Color Fill = UiKit.Jade;
    public Color Back = new("#22364a");

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Back);
        float w = Mathf.Clamp(Ratio, 0f, 1f) * Size.X;
        if (w > 0.5f) DrawRect(new Rect2(Vector2.Zero, new Vector2(w, Size.Y)), Fill);
    }
}
