using Godot;

namespace XianxiaRPG.UI;

/// <summary>立绘视图：优先显示位图（res://assets 下生成图），缺失时回退程序化头像。
/// 受击时 HurtFlash() 触发红色叠加层，随时间衰减淡出。</summary>
public partial class PortraitView : Control
{
    public Texture2D Tex;
    public string Title = "";
    public Color Robe = UiKit.Jade;
    private float _hurtT;
    private float _shakeT;
    private float _chargeT;

    public void HurtFlash()
    {
        _hurtT = 1f;
        QueueRedraw();
    }

    public void Shake()
    {
        _shakeT = 1f;
        QueueRedraw();
    }

    public void Charge()
    {
        _chargeT = 1f;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_hurtT > 0)
        {
            _hurtT = Mathf.Max(0, _hurtT - (float)delta * 2.5f);
            QueueRedraw();
        }
        if (_shakeT > 0)
        {
            _shakeT = Mathf.Max(0, _shakeT - (float)delta * 3f);
            QueueRedraw();
        }
        if (_chargeT > 0)
        {
            _chargeT = Mathf.Max(0, _chargeT - (float)delta * 1.6f);
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        var off = _shakeT > 0
            ? new Vector2((GD.Randf() * 2f - 1f) * 7f * _shakeT, (GD.Randf() * 2f - 1f) * 7f * _shakeT)
            : Vector2.Zero;

        if (Tex != null)
        {
            float ar = Tex.GetWidth() / (float)Tex.GetHeight();
            float w = Size.X, h = w / ar;
            if (h > Size.Y) { h = Size.Y; w = h * ar; }
            var rect = new Rect2(off.X + (Size.X - w) / 2f, off.Y + (Size.Y - h) / 2f, w, h);
            DrawTextureRect(Tex, rect, false);
        }
        else
            UiKit.DrawPortrait(this, new Vector2(Size.X / 2f, Size.Y / 2f) + off, Mathf.Min(Size.X, Size.Y) / 2f - 6, Robe, Title);

        if (_hurtT > 0)
        {
            var rc = new Color(0.9f, 0.1f, 0.1f, 0.5f * _hurtT);
            DrawRect(new Rect2(Vector2.Zero, Size), rc);
        }

        if (_chargeT > 0)
        {
            float pulse = 0.6f + 0.4f * Mathf.Sin((float)Time.GetTicksMsec() / 110f);
            float a = _chargeT * pulse;
            var bw = 4f;
            var col = new Color(1f, 0.85f, 0.4f, 0.9f * a);
            DrawRect(new Rect2(0, 0, Size.X, bw), col, true);
            DrawRect(new Rect2(0, Size.Y - bw, Size.X, bw), col, true);
            DrawRect(new Rect2(0, 0, bw, Size.Y), col, true);
            DrawRect(new Rect2(Size.X - bw, 0, bw, Size.Y), col, true);
            DrawRect(new Rect2(Vector2.Zero, Size), new Color(1f, 0.85f, 0.4f, 0.12f * a), true);
        }
    }
}
