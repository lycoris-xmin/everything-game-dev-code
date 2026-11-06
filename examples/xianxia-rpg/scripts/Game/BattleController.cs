using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using XianxiaRPG.Systems.Battle;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Loot;
using XianxiaRPG.UI;

/// <summary>
/// M5 战斗 HUD（仙剑式 3D 武侠布局）：
/// 左竖排队伍头像 + 血/蓝条，顶部 Boss 长血条，中间左我方立绘 / 右敌方立绘，
/// 右竖排可点击技能条（图标/名称/描述/消耗），底部滚动战斗日志。
/// 水墨国风：深色半透明面板 + 金/红/青高亮。
/// </summary>
[GlobalClass]
public partial class BattleController : Control
{
    public Main Root;
    private List<Combatant> _allies = new();
    private List<Combatant> _enemies = new();
    private List<Combatant> _all = new();
    private BattleLog _log = new();
    private int _tick;
    private readonly ICombatAI _ai = new SimpleAI();
    private Label _status;
    private VBoxContainer _logBox;
    private PortraitView _heroPortrait;
    private HBoxContainer _enemyRow;
    private readonly List<(Panel panel, PortraitView pv, BarView hp, Label nm, Combatant enemy)> _enemyRows = new();
    private Combatant _selectedEnemy;
    private readonly List<Button> _skillBtns = new();
    private readonly List<FloatText> _floaters = new();
    private FloatLayer _floatLayer;
    private Combatant _currentPlayer;
    private int _spotId;
    private bool _ended;

    // UI 引用
    private BarView _bossHpBar;
    private Label _bossHpLabel, _bossName;
    private PortraitView _bossIcon;
    private readonly List<(Panel panel, BarView hp, BarView mp, Label name)> _partyRows = new();

    public override void _Ready()
    {
        AnchorRight = 1; AnchorBottom = 1;

        UiKit.StackBg(this, "pixel/bg_sky.png", "pixel/bg_far.png", "pixel/bg_mid.png", "pixel/bg_near.png");

        var veil = new ColorRect { Color = new Color(0.02f, 0.04f, 0.06f, 0.5f) };
        veil.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(veil);

        var rootPad = new MarginContainer();
        rootPad.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        rootPad.AddThemeConstantOverride("margin_left", 22);
        rootPad.AddThemeConstantOverride("margin_right", 22);
        rootPad.AddThemeConstantOverride("margin_top", 18);
        rootPad.AddThemeConstantOverride("margin_bottom", 18);
        AddChild(rootPad);

        var mainRow = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        mainRow.AddThemeConstantOverride("separation", 20);
        rootPad.AddChild(mainRow);

        // 左侧：队伍面板
        var partyPanel = new VBoxContainer { CustomMinimumSize = new Vector2(210, 0) };
        partyPanel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanel, UiKit.BgPanelSoft));
        mainRow.AddChild(partyPanel);
        partyPanel.AddChild(UiKit.MakeLabel("队 伍", UiKit.Gold, 20));
        var partyRowsHost = new VBoxContainer();
        partyRowsHost.AddThemeConstantOverride("separation", 10);
        partyPanel.AddChild(partyRowsHost);
        _partyHost = partyRowsHost;

        // 中央：Boss血条 + 舞台 + 日志
        var center = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        center.AddThemeConstantOverride("separation", 14);
        mainRow.AddChild(center);

        BuildBossBar(center);

        _status = UiKit.MakeLabel("", UiKit.TextDim, 16);
        _status.HorizontalAlignment = HorizontalAlignment.Center;
        _status.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        center.AddChild(_status);

        var stage = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        stage.AddThemeConstantOverride("separation", 0);
        center.AddChild(stage);

        _heroPortrait = new PortraitView
        {
            Title = "云无月", Robe = UiKit.Jade,
            CustomMinimumSize = new Vector2(280, 380),
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        stage.AddChild(_heroPortrait);

        var midSpacer = new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        stage.AddChild(midSpacer);

        _enemyRow = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        _enemyRow.AddThemeConstantOverride("separation", 16);
        stage.AddChild(_enemyRow);

        var logPanel = new VBoxContainer { CustomMinimumSize = new Vector2(0, 110) };
        logPanel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanel, UiKit.BgPanelSoft));
        center.AddChild(logPanel);
        _logBox = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        _logBox.AddThemeConstantOverride("separation", 2);
        logPanel.AddChild(_logBox);

        // 右侧：招式面板
        var skillPanel = new VBoxContainer { CustomMinimumSize = new Vector2(260, 0) };
        skillPanel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanel, UiKit.BgPanelSoft));
        mainRow.AddChild(skillPanel);
        skillPanel.AddChild(UiKit.MakeLabel("招 式", UiKit.Gold, 20));
        var skillRowsHost = new VBoxContainer();
        skillRowsHost.AddThemeConstantOverride("separation", 10);
        skillPanel.AddChild(skillRowsHost);
        _skillHost = skillRowsHost;

        _floatLayer = new FloatLayer { Items = _floaters };
        AddChild(_floatLayer);
    }

    private VBoxContainer _partyHost, _skillHost;

    void BuildBossBar(VBoxContainer parent)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        parent.AddChild(row);

        var icon = _bossIcon = new PortraitView { CustomMinimumSize = new Vector2(48, 48), Robe = UiKit.Crimson, SizeFlagsVertical = SizeFlags.ShrinkCenter };
        icon.Name = "BossIcon";
        row.AddChild(icon);

        var vb = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkCenter };
        vb.AddThemeConstantOverride("separation", 4);
        row.AddChild(vb);

        _bossName = UiKit.MakeLabel("", UiKit.TextCol, 18);
        vb.AddChild(_bossName);

        var barRow = new HBoxContainer();
        barRow.AddThemeConstantOverride("separation", 8);
        barRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(barRow);

        _bossHpBar = new BarView { Fill = UiKit.Crimson, CustomMinimumSize = new Vector2(0, 18), SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkCenter };
        barRow.AddChild(_bossHpBar);
        _bossHpLabel = UiKit.MakeLabel("", UiKit.TextCol, 16);
        barRow.AddChild(_bossHpLabel);
    }

    void BuildPartyRows()
    {
        foreach (var c in _partyHost.GetChildren()) c.QueueFree();
        _partyRows.Clear();
        foreach (var ally in _allies)
        {
            var panel = new Panel();
            panel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanelSoft, UiKit.BgPanelSoft));
            panel.CustomMinimumSize = new Vector2(0, 86);
            _partyHost.AddChild(panel);

            var hb = new HBoxContainer();
            hb.AddThemeConstantOverride("separation", 10);
            hb.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            hb.AddThemeConstantOverride("margin_left", 8);
            hb.AddThemeConstantOverride("margin_right", 8);
            hb.AddThemeConstantOverride("margin_top", 8);
            hb.AddThemeConstantOverride("margin_bottom", 8);
            panel.AddChild(hb);

            var face = new PortraitView { CustomMinimumSize = new Vector2(60, 60), Title = ally.Name, Robe = UiKit.Jade, SizeFlagsVertical = SizeFlags.ShrinkCenter };
            face.Tex = UiKit.LoadTex("hero.png");
            face.QueueRedraw();
            hb.AddChild(face);

            var info = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkCenter };
            info.AddThemeConstantOverride("separation", 4);
            hb.AddChild(info);

            var nm = UiKit.MakeLabel(ally.Name, UiKit.TextCol, 16);
            info.AddChild(nm);
            var hp = new BarView { Fill = UiKit.HpCol, CustomMinimumSize = new Vector2(0, 8) };
            info.AddChild(hp);
            var mp = new BarView { Fill = UiKit.MpCol, CustomMinimumSize = new Vector2(0, 8) };
            info.AddChild(mp);

            _partyRows.Add((panel, hp, mp, nm));
        }
    }

    void BuildSkillRows()
    {
        foreach (var c in _skillHost.GetChildren()) c.QueueFree();
        _skillBtns.Clear();
        var hero = _allies[0];
        for (int i = 0; i < hero.Skills.Count; i++)
        {
            var s = hero.Skills[i];
            var btn = UiKit.MakeButtonPixel(s.Name, null, UiKit.Jade);
            btn.CustomMinimumSize = new Vector2(0, 64);
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _skillHost.AddChild(btn);
            _skillBtns.Add(btn);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);
            row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.SizeFlagsVertical = SizeFlags.ExpandFill;
            row.MouseFilter = MouseFilterEnum.Ignore;
            btn.AddChild(row);

            var iconTex = s.Kind == SkillKind.Ultimate ? UiKit.LoadTex("ic_ult.png")
                : s.Id == "art" ? UiKit.LoadTex("ic_skill.png")
                : UiKit.LoadTex("ic_atk.png");
            var icon = new TextureRect
            {
                Texture = iconTex,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(36, 36),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            row.AddChild(icon);

            var info = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkCenter };
            info.AddThemeConstantOverride("separation", 2);
            row.AddChild(info);

            var name = UiKit.MakeLabel(s.Name, UiKit.TextCol, 17);
            info.AddChild(name);
            var desc = UiKit.MakeLabel(string.IsNullOrEmpty(s.Description) ? "" : s.Description, UiKit.TextDim, 13);
            info.AddChild(desc);

            string costText;
            Color costColor;
            if (s.Kind == SkillKind.Ultimate)
            {
                costText = "神力";
                costColor = UiKit.UltCol;
            }
            else
            {
                costText = s.MpCost == 0 ? "—" : $"{s.MpCost}灵";
                costColor = UiKit.Jade;
            }
            var cost = UiKit.MakeLabel(costText, costColor, 15);
            cost.HorizontalAlignment = HorizontalAlignment.Right;
            cost.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
            cost.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            cost.CustomMinimumSize = new Vector2(40, 0);
            row.AddChild(cost);

            btn.Pressed += () => PlayerAct(s);
        }

        var back = UiKit.MakeButtonPixel("返回世界");
        back.Pressed += () => Root.EnterWorld();
        back.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _skillHost.AddChild(back);
        _skillBtns.Add(back);
    }

    public void Start(int spotId)
    {
        _spotId = spotId;
        _ended = false;
        _floaters.Clear();
        _allies = Root.Prog.Party.Select(c => c.Clone()).ToList();
        var spot = Root.Prog.CurrentRegion.Spots.First(s => s.Id == spotId);
        _enemies = spot.Enemies.Select(c => c.Clone()).ToList();
        _all = _allies.Concat(_enemies).ToList();
        _log = new BattleLog(); _tick = 0;
        _heroPortrait.Tex = UiKit.LoadTex("hero.png");
        _heroPortrait.Title = _allies[0].Name;
        _heroPortrait.QueueRedraw();
        BuildPartyRows();
        BuildEnemyRow();
        BuildSkillRows();
        Advance();
    }

    void Advance()
    {
        if (_ended) return;
        if (_allies.All(a => !a.Alive) || _enemies.All(e => !e.Alive)) { EndBattle(); return; }
        var actor = BattleEngine.PeekNextActor(_all);
        if (actor == null) { EndBattle(); return; }
        var foes = (actor.IsEnemy ? _allies : _enemies).Where(f => f.Alive).ToList();
        if (actor.IsEnemy)
        {
            _selectedEnemy = actor;
            foreach (var b in _skillBtns) b.Disabled = true;
            Refresh();
            Telegraph(actor, () =>
            {
                Skill skill = _ai.WantsUltimate(actor) ? actor.Skills.First(x => x.Kind == SkillKind.Ultimate) : _ai.ChooseSkill(actor);
                if (skill.MpCost > actor.Mp) skill = actor.Skills.First(x => x.Kind == SkillKind.Attack && x.MpCost == 0);
                var target = _ai.ChooseTarget(actor, foes);
                DoAction(actor, skill, target);
                if (!_ended) GetTree().CreateTimer(0.55).Timeout += Advance;
            });
        }
        else
        {
            _currentPlayer = actor;
            foreach (var b in _skillBtns) b.Disabled = false;
            RefreshButtons();
            Refresh();
        }
    }

    /// <summary>敌人行动预兆：立绘泛金光「蓄力」并短暂停顿，让玩家读出下一步，再执行出招。</summary>
    async void Telegraph(Combatant actor, Action onReady)
    {
        var er = _enemyRows.FirstOrDefault(r => r.enemy == actor);
        if (er.pv != null) er.pv.Charge();
        _status.Text = $"{actor.Name} 蓄力…";
        await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        if (!_ended) onReady();
    }

    /// <summary>执行一次行动并在目标立绘处弹出飘字（敌方受击偏右、我方受击偏左，击破高亮）。</summary>
    void DoAction(Combatant actor, Skill skill, Combatant target)
    {
        BattleEngine.ApplyAction(actor, skill, target, _all, _log, ref _tick);
        var ev = _log.Events[^1];
        var row = _enemyRows.FirstOrDefault(r => r.enemy == target);
        var at = target.IsEnemy && row.pv != null
            ? row.pv.GlobalPosition + new Vector2(row.pv.Size.X * 0.5f, row.pv.Size.Y * 0.25f)
            : _heroPortrait.GlobalPosition + new Vector2(_heroPortrait.Size.X * 0.5f, _heroPortrait.Size.Y * 0.25f);
        if (target.IsEnemy)
        {
            var trow = _enemyRows.FirstOrDefault(r => r.enemy == target);
            if (trow.pv != null) { trow.pv.HurtFlash(); trow.pv.Shake(); }
        }
        else
        {
            _heroPortrait.HurtFlash();
            _heroPortrait.Shake();
        }
        if (actor.IsEnemy)
        {
            var er = _enemyRows.FirstOrDefault(r => r.enemy == actor);
            if (er.pv != null) Lunge(er.pv, -34);
        }
        else
        {
            Lunge(_heroPortrait, 34);
        }
        string txt;
        Color col;
        if (ev.Action == "broken")
        {
            txt = "击破!";
            col = UiKit.UltCol;
        }
        else
        {
            txt = ev.Damage.ToString();
            col = target.IsEnemy ? UiKit.Crimson : UiKit.HpCol;
        }
        PushFloat(at, txt, col);
        Refresh();
    }

    void BuildEnemyRow()
    {
        foreach (var c in _enemyRow.GetChildren()) c.QueueFree();
        _enemyRows.Clear();
        _selectedEnemy = _enemies.FirstOrDefault(e => e.Alive);
        foreach (var e in _enemies)
        {
            var panel = new Panel();
            panel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanelSoft, UiKit.BgPanelSoft));
            panel.CustomMinimumSize = new Vector2(170, 250);
            _enemyRow.AddChild(panel);

            var vb = new VBoxContainer();
            vb.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            vb.AddThemeConstantOverride("margin_left", 8);
            vb.AddThemeConstantOverride("margin_right", 8);
            vb.AddThemeConstantOverride("margin_top", 8);
            vb.AddThemeConstantOverride("margin_bottom", 8);
            vb.AddThemeConstantOverride("separation", 6);
            panel.AddChild(vb);

            var pv = new PortraitView { Title = e.Name, Robe = UiKit.Crimson, CustomMinimumSize = new Vector2(150, 190), SizeFlagsVertical = SizeFlags.ShrinkBegin };
            pv.Tex = EnemyTex(e.Name);
            pv.QueueRedraw();
            vb.AddChild(pv);

            var hp = new BarView { Fill = UiKit.Crimson, CustomMinimumSize = new Vector2(0, 8) };
            vb.AddChild(hp);

            var nm = UiKit.MakeLabel(e.Name, UiKit.TextDim, 13);
            nm.HorizontalAlignment = HorizontalAlignment.Center;
            vb.AddChild(nm);

            var captured = e;
            panel.GuiInput += (evt) =>
            {
                if (evt is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left && captured.Alive)
                {
                    _selectedEnemy = captured;
                    Refresh();
                }
            };

            _enemyRows.Add((panel, pv, hp, nm, captured));
        }
    }

    static Texture2D EnemyTex(string name)
    {
        if (name.Contains("狼")) return UiKit.LoadTex("enemy_wolf.png");
        if (name.Contains("藤")) return UiKit.LoadTex("enemy_vine.png");
        if (name.Contains("蛛")) return UiKit.LoadTex("enemy_spider.png");
        return null;
    }

    void PlayerAct(Skill skill)
    {
        var foes = _enemies.Where(f => f.Alive).ToList();
        if (foes.Count == 0 || _currentPlayer == null) return;
        if (skill.Kind != SkillKind.Ultimate && _currentPlayer.Mp < skill.MpCost) return;
        if (skill.Kind == SkillKind.Ultimate && _currentPlayer.Ultimate < 100) return;
        var target = (foes.FirstOrDefault(f => f == _selectedEnemy) ?? foes.OrderBy(f => f.Hp).First());
        DoAction(_currentPlayer, skill, target);
        Advance();
    }

    public override void _Process(double delta)
    {
        if (_floaters.Count == 0) return;
        for (int i = 0; i < _floaters.Count; i++) { var f = _floaters[i]; f.Age += (float)delta; _floaters[i] = f; }
        _floaters.RemoveAll(f => f.Age > 1.1f);
        _floatLayer.QueueRedraw();
    }

    void PushFloat(Vector2 at, string text, Color col)
    {
        _floaters.Add(new FloatText { Start = at, Text = text, Col = col, Age = 0 });
        _floatLayer.QueueRedraw();
    }

    /// <summary>出招突进演出：将立绘沿 x 轴短促前冲再回弹。</summary>
    void Lunge(Control c, float dx)
    {
        float startX = c.Position.X;
        var t = CreateTween();
        t.TweenProperty(c, "position:x", startX + dx, 0.1);
        t.TweenProperty(c, "position:x", startX, 0.28);
    }

    void RefreshButtons()
    {
        var hero = _allies[0];
        for (int i = 0; i < hero.Skills.Count; i++)
        {
            var s = hero.Skills[i];
            _skillBtns[i].Disabled = s.Kind == SkillKind.Ultimate ? hero.Ultimate < 100 : hero.Mp < s.MpCost;
        }
    }

    void Refresh()
    {
        var hero = _allies[0];
        _status.Text = $"回合 {_tick}  ·  {hero.Name} 行动";

        // 队伍条
        for (int i = 0; i < _allies.Count; i++)
        {
            var a = _allies[i];
            var row = _partyRows[i];
            row.hp.Ratio = (float)a.Hp / a.MaxHp; row.hp.QueueRedraw();
            row.mp.Ratio = (float)a.Mp / a.MaxMp; row.mp.QueueRedraw();
            bool active = _currentPlayer == a && Visible;
            row.panel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanelSoft, active ? UiKit.Gold : UiKit.BgPanelSoft));
        }

        // 敌人立绘行：血条 + 选中金框 + 阵亡隐藏
        foreach (var r in _enemyRows)
        {
            if (!r.enemy.Alive)
            {
                r.panel.Visible = false;
                continue;
            }
            r.hp.Ratio = (float)r.enemy.Hp / r.enemy.MaxHp; r.hp.QueueRedraw();
            bool sel = _selectedEnemy == r.enemy;
            r.panel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanelSoft, sel ? UiKit.Gold : UiKit.BgPanelSoft));
            r.nm.AddThemeColorOverride("font_color", sel ? UiKit.Gold : UiKit.TextDim);
        }

        // Boss 条（跟随选中敌人，否则首个存活敌人）
        var boss = (_selectedEnemy != null && _selectedEnemy.Alive ? _selectedEnemy : _enemies.FirstOrDefault(e => e.Alive));
        if (boss != null)
        {
            _bossName.Text = boss.Name;
            _bossIcon.Tex = EnemyTex(boss.Name);
            _bossIcon.QueueRedraw();
            _bossHpBar.Ratio = (float)boss.Hp / boss.MaxHp; _bossHpBar.QueueRedraw();
            _bossHpLabel.Text = $"{boss.Hp}/{boss.MaxHp}";
        }
        else
        {
            _bossName.Text = "";
            _bossHpBar.Ratio = 0; _bossHpBar.QueueRedraw();
            _bossHpLabel.Text = "";
        }

        // 日志
        _logBox.GetChildren().ToList().ForEach(c => c.QueueFree());
        foreach (var ev in _log.Events.TakeLast(8))
            _logBox.AddChild(UiKit.MakeLabel($"{ev.Tick} {ev.Actor} {ev.Action} → {ev.Target} {ev.Damage}", UiKit.TextCol, 14));
    }

    void EndBattle()
    {
        _ended = true;
        bool win = _enemies.All(e => !e.Alive);
        var bus = Root.GetNode<EventBus>("/root/EventBus");
        if (win)
        {
            foreach (var e in _enemies)
                foreach (var m in DropSystem.RollDrops(e))
                {
                    Root.Prog.Inv.AddMaterial(m);
                    bus.EmitSignal(EventBus.SignalName.MaterialGained, m.Id, m.Stack);
                }
            Root.Prog.ClearedSpots.Add(_spotId);
            var hero = Root.Prog.Party[0];
            hero.Hp = hero.MaxHp; hero.Mp = hero.MaxMp;
            bus.EmitSignal(EventBus.SignalName.BattleEnded, true);
        }
        else
        {
            bus.EmitSignal(EventBus.SignalName.BattleEnded, false);
        }
        ShowResult(win);
    }

    /// <summary>战斗结算弹窗：胜/负标题与说明 + 「返回世界」按钮。点击后销毁弹窗并回到世界。</summary>
    void ShowResult(bool win)
    {
        var overlay = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var veil = new ColorRect { Color = new Color(0f, 0f, 0f, 0.55f) };
        veil.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.AddChild(veil);

        var cc = new CenterContainer();
        cc.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.AddChild(cc);
        var panel = new Panel();
        panel.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanel, UiKit.Gold));
        panel.CustomMinimumSize = new Vector2(460, 240);
        cc.AddChild(panel);

        var vb = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        vb.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        vb.AddThemeConstantOverride("separation", 18);
        vb.AddThemeConstantOverride("margin_left", 28);
        vb.AddThemeConstantOverride("margin_right", 28);
        vb.AddThemeConstantOverride("margin_top", 24);
        vb.AddThemeConstantOverride("margin_bottom", 24);
        panel.AddChild(vb);

        var head = UiKit.MakeLabel(win ? "胜  利" : "败  北", win ? UiKit.Gold : UiKit.Crimson, 40);
        head.HorizontalAlignment = HorizontalAlignment.Center;
        head.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(head);
        var desc = UiKit.MakeLabel(win ? "击破来犯之敌，掉落材料已入库" : "道心受挫，再接再厉", UiKit.TextDim, 18);
        desc.HorizontalAlignment = HorizontalAlignment.Center;
        desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vb.AddChild(desc);

        var btn = UiKit.MakeButtonPixel("返回世界", null, win ? UiKit.Jade : UiKit.Crimson);
        btn.Pressed += () => { overlay.QueueFree(); Root.EnterWorld(); };
        vb.AddChild(btn);

        veil.Modulate = new Color(1, 1, 1, 0);
        panel.PivotOffset = new Vector2(230, 120);
        panel.Scale = new Vector2(0.85f, 0.85f);
        var tw = CreateTween();
        tw.TweenProperty(veil, "modulate:a", 1f, 0.22);
        tw.Parallel().TweenProperty(panel, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out);

        AddChild(overlay);
    }
}
