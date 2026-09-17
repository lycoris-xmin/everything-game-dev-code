using Godot;
using System.Collections.Generic;
using System.Linq;
using XianxiaRPG.Systems.Battle;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.Systems.Entities;
using XianxiaRPG.Systems.Loot;
using XianxiaRPG.UI;

/// <summary>
/// M5 战斗 HUD（仙侠风）：左我方立绘 / 右敌方立绘，技能按钮带真实徽章图标，
/// HP/MP/神力用彩色进度条，下方滚动战斗日志。受击时飘字上浮淡出，终结技神力未满灰显。
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
    private VBoxContainer _btnRow;
    private VBoxContainer _logBox;
    private PortraitView _heroPortrait;
    private PortraitView _enemyPortrait;
    private BarView _hpBar, _mpBar, _ultBar;
    private readonly List<Button> _skillBtns = new();
    private readonly List<FloatText> _floaters = new();
    private FloatLayer _floatLayer;
    private Combatant _currentPlayer;
    private int _spotId;

    public override void _Ready()
    {
        AnchorRight = 1; AnchorBottom = 1;
        var bg = new Panel();
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        bg.AddThemeStyleboxOverride("panel", UiKit.Panel(UiKit.BgPanel, 0));
        AddChild(bg);

        var pad = new MarginContainer();
        pad.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        pad.AddThemeConstantOverride("margin_left", 28);
        pad.AddThemeConstantOverride("margin_right", 28);
        pad.AddThemeConstantOverride("margin_top", 24);
        pad.AddThemeConstantOverride("margin_bottom", 24);
        AddChild(pad);
        var vb = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        vb.AddThemeConstantOverride("separation", 14);
        pad.AddChild(vb);

        var stage = new HBoxContainer();
        stage.AddThemeConstantOverride("separation", 40);
        _heroPortrait = new PortraitView { Title = "云无月", Robe = UiKit.Jade, CustomMinimumSize = new Vector2(240, 320) };
        _enemyPortrait = new PortraitView { Title = "", Robe = UiKit.Crimson, CustomMinimumSize = new Vector2(240, 320) };
        stage.AddChild(_heroPortrait);
        stage.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        stage.AddChild(_enemyPortrait);
        vb.AddChild(stage);

        _status = UiKit.MakeLabel("", UiKit.TextCol, 18);
        vb.AddChild(_status);

        var bars = new VBoxContainer();
        bars.AddThemeConstantOverride("separation", 6);
        bars.AddChild(MakeBar("生命", out _hpBar, UiKit.HpCol));
        bars.AddChild(MakeBar("灵力", out _mpBar, UiKit.MpCol));
        bars.AddChild(MakeBar("神力", out _ultBar, UiKit.UltCol));
        vb.AddChild(bars);

        _btnRow = new VBoxContainer();
        _btnRow.AddThemeConstantOverride("separation", 8);
        vb.AddChild(_btnRow);

        _logBox = new VBoxContainer();
        _logBox.AddThemeConstantOverride("separation", 2);
        vb.AddChild(_logBox);

        _floatLayer = new FloatLayer { Items = _floaters };
        AddChild(_floatLayer);
    }

    static HBoxContainer MakeBar(string name, out BarView bar, Color col)
    {
        var h = new HBoxContainer();
        h.AddChild(UiKit.MakeLabel(name, UiKit.TextDim, 16));
        bar = new BarView
        {
            Fill = col,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(0, 18),
        };
        h.AddChild(bar);
        return h;
    }

    public void Start(int spotId)
    {
        _spotId = spotId;
        _floaters.Clear();
        _allies = Root.Prog.Party.Select(c => c.Clone()).ToList();
        var spot = Root.Prog.CurrentRegion.Spots.First(s => s.Id == spotId);
        _enemies = spot.Enemies.Select(c => c.Clone()).ToList();
        _all = _allies.Concat(_enemies).ToList();
        _log = new BattleLog(); _tick = 0;
        _heroPortrait.Tex = UiKit.LoadTex("hero.png");
        _heroPortrait.Title = _allies[0].Name;
        _heroPortrait.QueueRedraw();
        BuildButtons();
        Advance();
    }

    void BuildButtons()
    {
        foreach (var b in _skillBtns) b.QueueFree();
        _skillBtns.Clear();
        foreach (var s in _allies[0].Skills)
        {
            var icon = s.Kind == SkillKind.Ultimate ? UiKit.LoadTex("ic_ult.png")
                : s.Id == "art" ? UiKit.LoadTex("ic_skill.png")
                : UiKit.LoadTex("ic_atk.png");
            var btn = UiKit.MakeButton(
                s.Name + (s.Kind == SkillKind.Ultimate ? "(神力满)" : $"({s.MpCost}MP)"),
                icon, s.Kind == SkillKind.Ultimate ? UiKit.UltCol : UiKit.Jade);
            btn.Pressed += () => PlayerAct(s);
            _btnRow.AddChild(btn);
            _skillBtns.Add(btn);
        }
        var back = UiKit.MakeButton("返回世界");
        back.Pressed += () => Root.EnterWorld();
        _btnRow.AddChild(back);
        _skillBtns.Add(back);
    }

    void Advance()
    {
        while (true)
        {
            if (_allies.All(a => !a.Alive) || _enemies.All(e => !e.Alive)) { EndBattle(); return; }
            var actor = BattleEngine.PeekNextActor(_all);
            if (actor == null) { EndBattle(); return; }
            var foes = (actor.IsEnemy ? _allies : _enemies).Where(f => f.Alive).ToList();
            if (actor.IsEnemy)
            {
                ShowEnemy(actor);
                Skill skill = _ai.WantsUltimate(actor) ? actor.Skills.First(x => x.Kind == SkillKind.Ultimate) : _ai.ChooseSkill(actor);
                if (skill.MpCost > actor.Mp) skill = actor.Skills.First(x => x.Kind == SkillKind.Attack && x.MpCost == 0);
                var target = _ai.ChooseTarget(actor, foes);
                DoAction(actor, skill, target);
            }
            else
            {
                _currentPlayer = actor;
                RefreshButtons();
                return;
            }
        }
    }

    /// <summary>执行一次行动并在目标立绘处弹出飘字（敌人受击偏右、我方受击偏左，击破高亮）。</summary>
    void DoAction(Combatant actor, Skill skill, Combatant target)
    {
        BattleEngine.ApplyAction(actor, skill, target, _all, _log, ref _tick);
        var ev = _log.Events[^1];
        var at = target.IsEnemy ? new Vector2(1770, 200) : new Vector2(150, 200);
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

    void ShowEnemy(Combatant e)
    {
        _enemyPortrait.Tex = EnemyTex(e.Name);
        _enemyPortrait.Title = e.Name;
        _enemyPortrait.Robe = UiKit.Crimson;
        _enemyPortrait.QueueRedraw();
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
        var target = foes.OrderBy(f => f.Hp).First();
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
        _status.Text = $"Tick {_tick}   我方 {hero.Name} HP{hero.Hp}/{hero.MaxHp} MP{hero.Mp}/{hero.MaxMp} 神力{hero.Ultimate}";
        _hpBar.Ratio = (float)hero.Hp / hero.MaxHp; _hpBar.QueueRedraw();
        _mpBar.Ratio = (float)hero.Mp / hero.MaxMp; _mpBar.QueueRedraw();
        _ultBar.Ratio = (float)hero.Ultimate / 100f; _ultBar.QueueRedraw();
        var firstEnemy = _enemies.FirstOrDefault(e => e.Alive);
        if (firstEnemy != null) ShowEnemy(firstEnemy);
        _logBox.GetChildren().ToList().ForEach(c => c.QueueFree());
        foreach (var ev in _log.Events.TakeLast(8))
            _logBox.AddChild(UiKit.MakeLabel($"{ev.Tick} {ev.Actor} {ev.Action} → {ev.Target} {ev.Damage}", UiKit.TextCol, 15));
    }

    void EndBattle()
    {
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
        Root.EnterWorld();
    }
}
