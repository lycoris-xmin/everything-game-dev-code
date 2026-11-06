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
            float pop = t < 0.18f ? Mathf.Lerp(1.7f, 1f, t / 0.18f) : 1f;
            int fsize = (int)(34 * pop);
            Vector2 p = f.Start + new Vector2(0, -56f * t);
            Color c = f.Col;
            c.A = Mathf.Clamp(1f - t * t, 0f, 1f);
            var size = font.GetStringSize(f.Text, HorizontalAlignment.Left, -1, fsize);
            Vector2 basePos = p - new Vector2(size.X / 2f, 0);
            DrawString(font, basePos + new Vector2(2, 2), f.Text, HorizontalAlignment.Left, -1, fsize, new Color(0, 0, 0, c.A * 0.5f));
            DrawString(font, basePos, f.Text, HorizontalAlignment.Left, -1, fsize, c);
        }
    }
}
