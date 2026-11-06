using Godot;
using System;
using System.Linq;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Crafting;
using XianxiaRPG.UI;

/// <summary>
/// M5 角色面板 / 打造界面（仙侠风）：左侧主角立绘，右侧属性与材料，下方打造配方（带装备图标）。
/// </summary>
[GlobalClass]
public partial class PartyPanel : Control
{
    public Main Root;
    private PortraitView _portrait;
    private Label _info;
    private VBoxContainer _matBox;
    private VBoxContainer _craftBtns;

    public override void _Ready()
    {
        AnchorRight = 1; AnchorBottom = 1;
        var bg = new Panel();
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        bg.AddThemeStyleboxOverride("panel", UiKit.PanelPixel(UiKit.BgPanel, UiKit.BgPanelSoft));
        AddChild(bg);
        var pad = new MarginContainer();
        pad.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        pad.AddThemeConstantOverride("margin_left", 28);
        pad.AddThemeConstantOverride("margin_right", 28);
        pad.AddThemeConstantOverride("margin_top", 24);
        pad.AddThemeConstantOverride("margin_bottom", 24);
        AddChild(pad);
        var vb = new VBoxContainer();
        vb.AddThemeConstantOverride("separation", 12);
        pad.AddChild(vb);

        var top = new HBoxContainer();
        top.AddThemeConstantOverride("separation", 20);
        _portrait = new PortraitView { Title = "云无月", Robe = UiKit.Jade, CustomMinimumSize = new Vector2(160, 220) };
        top.AddChild(_portrait);
        _info = UiKit.MakeLabel("", UiKit.TextCol, 18);
        top.AddChild(_info);
        vb.AddChild(top);

        vb.AddChild(UiKit.MakeLabel("材料", UiKit.Gold, 18));
        _matBox = new VBoxContainer();
        _matBox.AddThemeConstantOverride("separation", 4);
        vb.AddChild(_matBox);

        vb.AddChild(UiKit.MakeLabel("打造（点击并穿戴）", UiKit.Gold, 18));
        _craftBtns = new VBoxContainer();
        _craftBtns.AddThemeConstantOverride("separation", 8);
        vb.AddChild(_craftBtns);

        var back = UiKit.MakeButtonPixel("返回世界（Esc）");
        back.Pressed += () => Root.EnterWorld();
        vb.AddChild(back);
    }

    public void Refresh()
    {
        var hero = Root.Prog.Party[0];
        _portrait.Tex = UiKit.LoadTex("hero.png");
        _portrait.Title = hero.Name;
        _portrait.QueueRedraw();
        _info.Text = $"{hero.Name}  Lv{Root.Prog.Level.Level}  元素 {hero.Element}\n" +
            $"HP {hero.Hp}/{hero.MaxHp}   MP {hero.Mp}/{hero.MaxMp}   神力 {hero.Ultimate}\n" +
            $"攻 {hero.Attr[AttrKey.Attack]}  防 {hero.Attr[AttrKey.Defense]}  灵力 {hero.Attr[AttrKey.MagicPower]}  灵抗 {hero.Attr[AttrKey.MagicResist]}  身法 {hero.Attr[AttrKey.Speed]}";

        foreach (var c in _matBox.GetChildren()) c.QueueFree();
        if (Root.Prog.Inv.Materials.Count == 0)
            _matBox.AddChild(UiKit.MakeLabel("（无）", UiKit.TextDim, 16));
        foreach (var m in Root.Prog.Inv.Materials)
        {
            var row = new HBoxContainer();
            row.AddChild(new DotView { Col = UiKit.Jade, CustomMinimumSize = new Vector2(20, 20) });
            row.AddChild(UiKit.MakeLabel($"{m.Name} x{m.Stack}", UiKit.TextCol, 16));
            _matBox.AddChild(row);
        }

        foreach (var c in _craftBtns.GetChildren()) c.QueueFree();
        foreach (var r in Root.Prog.Recipes)
        {
            bool can = CraftingSystem.CanCraft(Root.Prog.Inv, r);
            var btn = UiKit.MakeButtonPixel(
                $"打造 {r.Name}（攻+{r.Bonus[AttrKey.Attack]}，需风精x{r.Cost["mat_wind"]}）",
                UiKit.LoadTex("sword.png"), can ? UiKit.Gold : null);
            btn.Disabled = !can;
            btn.Pressed += () =>
            {
                if (CraftingSystem.Craft(Root.Prog.Inv, r) != null) { ApplyEquip(r); Refresh(); }
            };
            _craftBtns.AddChild(btn);
        }
    }

    void ApplyEquip(Equipment r)
    {
        var hero = Root.Prog.Party[0];
        foreach (AttrKey k in Enum.GetValues<AttrKey>())
            if (r.Bonus[k] != 0) hero.Attr.AddBonus(k, r.Bonus[k]);
    }
}
