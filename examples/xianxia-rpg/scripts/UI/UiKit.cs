using Godot;

namespace XianxiaRPG.UI;

/// <summary>
/// 仙侠风 UI 共享套件：配色、面板样式、纹理加载与程序化绘制辅助。
/// 所有面板（World/Battle/Party）复用同一套国风深色主题，保证视觉一致。
/// 资源缺失时（例如未生成位图）自动回退到程序化绘制，保证可运行。
/// </summary>
public static class UiKit
{
    public static readonly Color BgDeep = new("#0b1622");
    public static readonly Color BgPanel = new("#13243a");
    public static readonly Color BgPanelSoft = new("#1b3550");
    public static readonly Color Gold = new("#e8c97a");
    public static readonly Color Jade = new("#5fd0b0");
    public static readonly Color Crimson = new("#e0556b");
    public static readonly Color HpCol = new("#e0556b");
    public static readonly Color MpCol = new("#5aa9e6");
    public static readonly Color UltCol = new("#7fe3ff");
    public static readonly Color TextCol = new("#e6eef5");
    public static readonly Color TextDim = new("#9fb3c8");

    public const string Assets = "res://assets/";

    /// <summary>按文件名加载 assets 下的纹理；文件不存在返回 null（不崩溃）。</summary>
    public static Texture2D LoadTex(string file) => GD.Load<Texture2D>(Assets + file);

    public static StyleBoxFlat Panel(Color fill, float radius = 14, Color? border = null)
    {
        var s = new StyleBoxFlat
        {
            BgColor = fill,
            CornerRadiusTopLeft = (int)radius,
            CornerRadiusTopRight = (int)radius,
            CornerRadiusBottomLeft = (int)radius,
            CornerRadiusBottomRight = (int)radius,
            ContentMarginLeft = 16, ContentMarginRight = 16, ContentMarginTop = 12, ContentMarginBottom = 12,
        };
        if (border.HasValue) { s.BorderColor = border.Value; s.SetBorderWidthAll(2); }
        return s;
    }

    public static Label MakeLabel(string text, Color? color = null, int size = 18)
    {
        var l = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Left };
        l.AddThemeColorOverride("font_color", color ?? TextCol);
        l.AddThemeFontSizeOverride("font_size", size);
        return l;
    }

    public static Button MakeButton(string text, Texture2D icon = null, Color? accent = null)
    {
        var b = new Button { Text = text };
        if (icon != null) b.Icon = icon;
        b.AddThemeFontSizeOverride("font_size", 18);
        b.AddThemeColorOverride("font_color", TextCol);
        var a = accent ?? BgPanelSoft;
        b.AddThemeStyleboxOverride("normal", Panel(a, 10, a));
        b.AddThemeStyleboxOverride("hover", Panel(Jade, 10, Gold));
        b.AddThemeStyleboxOverride("pressed", Panel(Gold, 10, Gold));
        b.AddThemeStyleboxOverride("disabled", Panel(new Color(0.2f, 0.25f, 0.3f, 1), 10, TextDim));
        return b;
    }

    /// <summary>无位图时的程序化立绘回退（圆形身躯 + 头 + 剑）。</summary>
    public static void DrawPortrait(CanvasItem ci, Vector2 c, float r, Color robe, string name)
    {
        ci.DrawCircle(c, r, robe);
        ci.DrawCircle(new Vector2(c.X, c.Y - r * 0.4f), r * 0.4f, new Color(1, 1, 1, 0.92f));
        ci.DrawLine(c + new Vector2(r * 0.25f, -r * 0.1f), c + new Vector2(r * 0.8f, -r * 0.95f), new Color(0.92f, 0.96f, 1f), 5);
        var font = ThemeDB.FallbackFont;
        if (font != null) ci.DrawString(font, c + new Vector2(-r * 0.5f, r + 16), name, HorizontalAlignment.Center, 0, 18, TextCol);
    }
}

/// <summary>战斗飘字数据：起始位置、文本、颜色、已存活时间（秒）。由 FloatLayer 渲染。</summary>
public struct FloatText
{
    public Vector2 Start;
    public string Text;
    public Color Col;
    public float Age;
}
