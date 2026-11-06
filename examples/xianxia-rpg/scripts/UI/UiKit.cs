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

    /// <summary>像素风面板：直角硬边 + 细描边（无圆角），贴合像素美术。</summary>
    public static StyleBoxFlat PanelPixel(Color fill, Color? border = null)
    {
        var s = new StyleBoxFlat
        {
            BgColor = fill,
            CornerRadiusTopLeft = 0, CornerRadiusTopRight = 0, CornerRadiusBottomLeft = 0, CornerRadiusBottomRight = 0,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 10, ContentMarginBottom = 10,
        };
        if (border.HasValue) { s.BorderColor = border.Value; s.SetBorderWidthAll(2); }
        return s;
    }

    /// <summary>像素风按钮：直角硬描边，hover 用玉青、按下用金。</summary>
    public static Button MakeButtonPixel(string text, Texture2D icon = null, Color? accent = null)
    {
        var b = new Button { Text = text };
        if (icon != null) b.Icon = icon;
        b.AddThemeFontSizeOverride("font_size", 18);
        b.AddThemeColorOverride("font_color", TextCol);
        var a = accent ?? BgPanelSoft;
        b.AddThemeStyleboxOverride("normal", PanelPixel(a, a));
        b.AddThemeStyleboxOverride("hover", PanelPixel(Jade, Gold));
        b.AddThemeStyleboxOverride("pressed", PanelPixel(Gold, Gold));
        b.AddThemeStyleboxOverride("disabled", PanelPixel(new Color(0.18f, 0.22f, 0.28f, 1), TextDim));
        return b;
    }

    /// <summary>堆叠视差背景层（远→近），用于国风 HD-2D 分层山水。按传入顺序叠放（后者在上）。</summary>
    public static void StackBg(Control host, params string[] layers)
    {
        foreach (var f in layers)
        {
            var tr = new TextureRect
            {
                Texture = LoadTex(f),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            };
            tr.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            tr.MouseFilter = Control.MouseFilterEnum.Ignore;
            host.AddChild(tr);
        }
    }

    /// <summary>程序化仙侠风水墨背景：暮色渐变天 + 落月 + 远/近山峦 + 雾带 + 地面。供战斗与标题共用。</summary>
    public static void DrawInkBackdrop(Control ci)
    {
        float w = ci.Size.X, h = ci.Size.Y;
        var skyTop = new Color(0.13f, 0.15f, 0.24f);
        var skyBot = new Color(0.30f, 0.20f, 0.18f);
        const int bands = 48;
        for (int i = 0; i < bands; i++)
        {
            float t = i / (float)bands;
            var col = skyTop.Lerp(skyBot, t);
            ci.DrawRect(new Rect2(0, h * i / bands, w, h / bands + 1), col, true);
        }
        ci.DrawCircle(new Vector2(w * 0.82f, h * 0.2f), Mathf.Min(w, h) * 0.06f, new Color(0.92f, 0.9f, 0.82f, 0.85f));
        ci.DrawColoredPolygon(new[]
        {
            new Vector2(0, h), new Vector2(0, h * 0.58f), new Vector2(w * 0.25f, h * 0.38f),
            new Vector2(w * 0.5f, h * 0.52f), new Vector2(w * 0.75f, h * 0.34f), new Vector2(w, h * 0.5f), new Vector2(w, h),
        }, new Color(0.22f, 0.24f, 0.32f, 0.9f));
        ci.DrawRect(new Rect2(0, h * 0.5f, w, h * 0.12f), new Color(0.85f, 0.85f, 0.9f, 0.12f), true);
        ci.DrawColoredPolygon(new[]
        {
            new Vector2(0, h), new Vector2(0, h * 0.74f), new Vector2(w * 0.3f, h * 0.54f),
            new Vector2(w * 0.55f, h * 0.7f), new Vector2(w * 0.8f, h * 0.54f), new Vector2(w, h * 0.68f), new Vector2(w, h),
        }, new Color(0.10f, 0.12f, 0.17f, 0.95f));
        var gTop = new Color(0.10f, 0.12f, 0.17f);
        var gBot = new Color(0.05f, 0.06f, 0.08f);
        const int gb = 24;
        for (int i = 0; i < gb; i++)
        {
            float t = i / (float)gb;
            var col = gTop.Lerp(gBot, t);
            float y = h * 0.8f + (h * 0.2f) * i / gb;
            ci.DrawRect(new Rect2(0, y, w, h * 0.2f / gb + 1), col, true);
        }
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
