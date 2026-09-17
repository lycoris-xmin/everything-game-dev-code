using Godot;

namespace XianxiaRPG.UI;

/// <summary>小圆点图标（用于材料/属性标记），纯程序化绘制。</summary>
public partial class DotView : Control
{
    public Color Col = UiKit.Jade;

    public override void _Draw()
    {
        DrawCircle(new Vector2(Size.X / 2f, Size.Y / 2f), Mathf.Min(Size.X, Size.Y) / 2f - 2, Col);
    }
}
