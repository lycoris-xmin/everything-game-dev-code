using Godot;

namespace XianxiaRPG.UI;

/// <summary>立绘视图：优先显示位图（res://assets 下生成图），缺失时回退程序化头像。</summary>
public partial class PortraitView : Control
{
    public Texture2D Tex;
    public string Title = "";
    public Color Robe = UiKit.Jade;

    public override void _Draw()
    {
        if (Tex != null)
        {
            DrawTextureRect(Tex, new Rect2(Vector2.Zero, Size), false);
            return;
        }
        UiKit.DrawPortrait(this, new Vector2(Size.X / 2f, Size.Y / 2f), Mathf.Min(Size.X, Size.Y) / 2f - 6, Robe, Title);
    }
}
