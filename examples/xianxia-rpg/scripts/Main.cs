using Godot;
using XianxiaRPG.Systems.Exploration;
using XianxiaRPG.UI;

/// <summary>
/// 游戏根节点（场景管理器）：在 Title / World / Battle / Party 四个面板间切换。
/// 逻辑层（M1..M5）通过 ProgressionState 唯一真相源驱动；存档经 SaveService。
/// </summary>
[GlobalClass]
public partial class Main : Node2D
{
    public ProgressionState Prog;
    public TitleScreen Title;
    public World2D World;
    public BattleController Battle;
    public PartyPanel Party;

    public override void _Ready()
    {
        Prog = GetNode<ProgressionState>("/root/ProgressionState");
        var save = GetNode<SaveService>("/root/SaveService");
        bool loaded = save.Load();
        if (!loaded) GD.Print("[Main] 无存档，使用默认进度");

        World = new World2D { Root = this };
        Battle = new BattleController { Root = this };
        Party = new PartyPanel { Root = this };
        Title = new TitleScreen { Root = this, HasSave = loaded };
        AddChild(World);
        AddChild(Battle);
        AddChild(Party);
        AddChild(Title);
        EnterTitle();
        GD.Print("仙侠回合制RPG — M1..M5 Godot 集成就绪");
    }

    public void EnterTitle()
    {
        World.Visible = false;
        Battle.Visible = false;
        Party.Visible = false;
        Title.Visible = true;
    }

    public void EnterBattle(int spotId)
    {
        Title.Visible = false;
        World.Visible = false;
        Battle.Start(spotId);
        Battle.Visible = true;
    }

    public void EnterWorld()
    {
        Title.Visible = false;
        Battle.Visible = false;
        Party.Visible = false;
        World.Refresh();
        World.Visible = true;
    }

    public void EnterParty()
    {
        Title.Visible = false;
        World.Visible = false;
        Battle.Visible = false;
        Party.Refresh();
        Party.Visible = true;
    }
}
