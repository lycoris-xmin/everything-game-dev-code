using Godot;
using System.Collections.Generic;

namespace XianxiaRPG.UI;

/// <summary>顶层飘字层：在战斗 HUD 最上方绘制受击数字 / 击破提示，随时间上浮并淡出。
/// 设为 Ignore 鼠标过滤，避免遮挡下方技能按钮的交互。</summary>
public partial class FloatLayer : Control
{
    public List<FloatText> Items;

    public override void _Ready()
    {
        MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        if (font == null || Items == null) return;
        foreach (var f in Items)
        {
            float t = f.Age / 1.1f;
            Vector2 p = f.Start + new Vector2(0, -50f * t);
            Color c = f.Col;
            c.A = Mathf.Clamp(1f - t, 0f, 1f);
            var size = font.GetStringSize(f.Text, HorizontalAlignment.Left, -1, 30);
            DrawString(font, p - new Vector2(size.X / 2f, 0), f.Text, HorizontalAlignment.Left, -1, 30, c);
        }
    }
}
