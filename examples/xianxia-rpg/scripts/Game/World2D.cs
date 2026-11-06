using Godot;
using System.Collections.Generic;
using XianxiaRPG.UI;

/// <summary>
/// M3 世界探索（2D 像素风，仙剑式俯视村落）：瓦片地图（草地/石径/水池/树/屋/石/花）+
/// 俯视斜角相机跟随，主角像素精灵 WASD 在 XZ(平面) 移动。遇敌采用仙剑式：接触红色精灵明雷 +
/// 走动概率触发的暗雷。Tab 开角色/打造面板。战斗/角色面板为 2D 覆盖层，由 Main 切换。
/// </summary>
[GlobalClass]
public partial class World2D : Node2D
{
    public Main Root;
    private Node2D _hero;
    private Camera2D _cam;
    private Control _hud;
    private Label _near;
    private readonly Dictionary<int, Node2D> _markers = new();
    private readonly Dictionary<int, Sprite2D> _auras = new();
    private float _encounterMeter;

    private const int TILE = 16;
    private const int ZOOM = 2;
    private const int COLS = 80, ROWS = 60;

    private Texture2D _tGrass, _tPath, _tWater, _tSand, _tTree, _tHouse, _tRock, _tFlower, _tHero, _shadowTex;

    public override void _Ready()
    {
        _tGrass = UiKit.LoadTex("pixel/tile_grass.png");
        _tPath = UiKit.LoadTex("pixel/tile_path.png");
        _tWater = UiKit.LoadTex("pixel/tile_water.png");
        _tSand = UiKit.LoadTex("pixel/tile_sand.png");
        _tTree = UiKit.LoadTex("pixel/tree.png");
        _tHouse = UiKit.LoadTex("pixel/house.png");
        _tRock = UiKit.LoadTex("pixel/rock.png");
        _tFlower = UiKit.LoadTex("pixel/flower.png");
        _tHero = UiKit.LoadTex("hero.png");
        _shadowTex = MakeShadow();

        // 夜空远山底幕（独立 CanvasLayer，永远在地图之后）
        var sky = new CanvasLayer { Layer = -10 };
        var skyTr = new TextureRect
        {
            Texture = UiKit.LoadTex("pixel/world_back.png"),
            ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        skyTr.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        sky.AddChild(skyTr);
        AddChild(sky);

        BuildMap();
        BuildHero();
        BuildCamera();
        BuildMarkers();
        BuildHud();
        Refresh();
    }

    void AddTile(Texture2D tex, int tx, int ty, bool centered = false)
    {
        var sp = new Sprite2D
        {
            Texture = tex,
            Centered = centered,
            Scale = Vector2.One * ZOOM,
            Position = new Vector2(tx * TILE * ZOOM, ty * TILE * ZOOM),
            ZIndex = centered ? ty + 1 : ty,
        };
        AddChild(sp);
    }

    void BuildMap()
    {
        for (int y = 0; y < ROWS; y++)
            for (int x = 0; x < COLS; x++)
            {
                // 水池（圆形）
                var dWater = Mathf.Sqrt(Mathf.Pow(x - 46, 2) + Mathf.Pow(y - 12, 2));
                // 石径十字
                bool path = (y == 22 && x > 3 && x < COLS - 3) || (x == 30 && y > 3 && y < ROWS - 4);
                Texture2D tex = _tGrass;
                bool centered = false;
                if (x == 0 || y == 0 || x == COLS - 1 || y == ROWS - 1) { tex = _tTree; centered = true; }
                else if (dWater < 6) tex = _tWater;
                else if (dWater < 7.5f) tex = _tSand;
                else if (path) tex = _tPath;
                AddTile(tex, x, y, centered);
            }

        // 固定装饰
        AddTile(_tHouse, 8, 8, true);
        AddTile(_tHouse, 50, 30, true);
        foreach (var (x, y) in new[] { (4, 6), (54, 8), (6, 36), (55, 36), (20, 38), (38, 4) })
            AddTile(_tTree, x, y, true);
        foreach (var (x, y) in new[] { (12, 26), (18, 18), (40, 28), (33, 33), (24, 30) })
            AddTile(_tRock, x, y, true);
        foreach (var (x, y) in new[] { (10, 20), (14, 24), (34, 18), (44, 26), (26, 34), (16, 30) })
            AddTile(_tFlower, x, y, true);
    }

    void BuildHero()
    {
        _hero = new Node2D();
        var sp = new Sprite2D { Texture = _tHero, Scale = Vector2.One * ZOOM, Centered = true, ZIndex = 500 };
        _hero.AddChild(sp);
        var sh = new Sprite2D { Texture = _shadowTex, Scale = Vector2.One * 0.9f, Position = new Vector2(0, 12 * ZOOM), ZIndex = 499, Modulate = new Color(1, 1, 1, 0.5f) };
        _hero.AddChild(sh);
        _hero.Position = new Vector2(30 * TILE * ZOOM, 22 * TILE * ZOOM);
        AddChild(_hero);
    }

    void BuildCamera()
    {
        _cam = new Camera2D { PositionSmoothingEnabled = true, PositionSmoothingSpeed = 6f };
        _cam.LimitLeft = 0; _cam.LimitTop = 0;
        _cam.LimitRight = COLS * TILE * ZOOM; _cam.LimitBottom = ROWS * TILE * ZOOM;
        AddChild(_cam);
    }

    void BuildMarkers()
    {
        foreach (var spot in Root.Prog.CurrentRegion.Spots)
        {
            var mk = new Node2D();
            mk.Position = SpotPos(spot.Id);
            var sp = new Sprite2D
            {
                Texture = EnemyTex(spot.Enemies[0].Name),
                Scale = Vector2.One * ZOOM,
                Centered = true,
                ZIndex = 6,
                Modulate = new Color(1, 1, 1, 0.92f),
            };
            mk.AddChild(sp);
            // 红色明雷光环
            var aura = new Sprite2D
            {
                Texture = MakeAura(),
                Scale = Vector2.One * 1.4f,
                Centered = true,
                ZIndex = 500,
                Modulate = new Color(1, 0.3f, 0.3f, 0.5f),
            };
            mk.AddChild(aura);
            var sh = new Sprite2D { Texture = _shadowTex, Scale = Vector2.One * 1.1f, Position = new Vector2(0, 12 * ZOOM), ZIndex = 499, Modulate = new Color(1, 1, 1, 0.5f) };
            mk.AddChild(sh);
            AddChild(mk);
            _markers[spot.Id] = mk;
            _auras[spot.Id] = aura;
        }
    }

    // 程序化生成一个红色软光环贴图（仅运行内存，不落盘）
    Texture2D MakeAura()
    {
        var im = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
        im.Fill(Colors.Transparent);
        for (int i = 0; i < 32; i++)
            for (int j = 0; j < 32; j++)
            {
                var d = Mathf.Sqrt(Mathf.Pow(i - 15.5f, 2) + Mathf.Pow(j - 15.5f, 2));
                if (d < 15) im.SetPixel(i, j, new Color(1, 0.3f, 0.3f, (float)(1 - d / 15) * 0.8f));
            }
        var tex = ImageTexture.CreateFromImage(im);
        return tex;
    }

    // 程序化生成柔和落地阴影（扁平椭圆，仅内存）
    Texture2D MakeShadow()
    {
        var im = Image.CreateEmpty(32, 16, false, Image.Format.Rgba8);
        im.Fill(Colors.Transparent);
        for (int yy = 0; yy < 16; yy++)
            for (int xx = 0; xx < 32; xx++)
            {
                var dx = (xx - 15.5f) / 15.5f;
                var dy = (yy - 7.5f) / 7.5f;
                var d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < 1) im.SetPixel(xx, yy, new Color(0, 0, 0, (float)(1 - d) * 0.55f));
            }
        return ImageTexture.CreateFromImage(im);
    }

    void BuildHud()
    {
        var layer = new CanvasLayer();
        AddChild(layer);
        _hud = new Control();
        _hud.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(_hud);
        var tip = UiKit.MakeLabel("WASD 移动 · 靠近红色精灵按空格遭遇（走动亦会随机遇敌）· Tab 开角色/打造", UiKit.TextDim, 18);
        tip.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        tip.AddThemeConstantOverride("margin_top", 16);
        tip.HorizontalAlignment = HorizontalAlignment.Center;
        _hud.AddChild(tip);

        _near = UiKit.MakeLabel("按 [空格] 遭遇！", UiKit.Gold, 22);
        _near.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
        _near.AddThemeConstantOverride("margin_bottom", 120);
        _near.HorizontalAlignment = HorizontalAlignment.Center;
        _near.Visible = false;
        _hud.AddChild(_near);
    }

    static Vector2 SpotPos(int id) => id switch
    {
        0 => new Vector2(12 * 16 * 2, 16 * 16 * 2),
        1 => new Vector2(44 * 16 * 2, 30 * 16 * 2),
        _ => new Vector2(20 * 16 * 2, 20 * 16 * 2),
    };

    static Texture2D EnemyTex(string name)
    {
        if (name.Contains("狼")) return UiKit.LoadTex("enemy_wolf.png");
        if (name.Contains("藤")) return UiKit.LoadTex("enemy_vine.png");
        if (name.Contains("蛛")) return UiKit.LoadTex("enemy_spider.png");
        return UiKit.LoadTex("enemy_wolf.png");
    }

    /// <summary>回到世界时刷新：已清遭遇点隐藏，并重置遇敌计数器。</summary>
    public void Refresh()
    {
        foreach (var (id, mk) in _markers)
            mk.Visible = !Root.Prog.ClearedSpots.Contains(id);
        _encounterMeter = 0;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;

        var dir = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W)) dir.Y -= 1;
        if (Input.IsKeyPressed(Key.S)) dir.Y += 1;
        if (Input.IsKeyPressed(Key.A)) dir.X -= 1;
        if (Input.IsKeyPressed(Key.D)) dir.X += 1;

        if (dir != Vector2.Zero)
        {
            dir = dir.Normalized();
            _hero.Position += dir * 8f * (float)delta * ZOOM;
            _hero.Position = new Vector2(
                Mathf.Clamp(_hero.Position.X, TILE * ZOOM, (COLS - 1) * TILE * ZOOM),
                Mathf.Clamp(_hero.Position.Y, TILE * ZOOM, (ROWS - 1) * TILE * ZOOM));

            _encounterMeter += (float)delta;
            if (_encounterMeter > 0.5f)
            {
                _encounterMeter = 0;
                if (GD.Randf() < 0.04f)
                {
                    var avail = Root.Prog.CurrentRegion.Spots
                        .FindAll(s => !Root.Prog.ClearedSpots.Contains(s.Id));
                    if (avail.Count > 0)
                    {
                        var pick = avail[(int)(GD.Randi() % (uint)avail.Count)];
                        Root.EnterBattle(pick.Id);
                        return;
                    }
                }
            }
        }
        else _encounterMeter = 0;

        _cam.GlobalPosition = _hero.GlobalPosition;

        float t = (float)Time.GetTicksMsec() / 400f;
        foreach (var (id, mk) in _markers)
        {
            if (!mk.Visible) continue;
            mk.Position = new Vector2(mk.Position.X, SpotPos(id).Y + Mathf.Sin(t) * 0.3f * ZOOM);
            if (_auras.TryGetValue(id, out var aura))
                aura.Modulate = new Color(1, 0.3f, 0.3f, 0.35f + 0.2f * Mathf.Sin(t * 2f));
        }

        int? near = null;
        foreach (var spot in Root.Prog.CurrentRegion.Spots)
        {
            if (Root.Prog.ClearedSpots.Contains(spot.Id)) continue;
            var p = SpotPos(spot.Id);
            var d = _hero.Position.DistanceTo(p);
            if (d < 3f * TILE * ZOOM) { near = spot.Id; break; }
        }
        _near.Visible = near.HasValue && !Input.IsKeyPressed(Key.Tab);
        if (near.HasValue)
        {
            if (Input.IsKeyPressed(Key.Space)) Root.EnterBattle(near.Value);
            else if (Input.IsKeyPressed(Key.Tab)) Root.EnterParty();
        }
    }
}
