using System.Collections.Generic;

namespace XianxiaRPG.Systems.Battle;

/// <summary>
/// 单条战斗事件，供日志回放与测试断言。
/// </summary>
public class BattleEvent
{
    public int Tick;
    public string Actor = "";
    public string Action = "";   // skill / ultimate / defend / broken / recover / death
    public string Target = "";
    public int Damage;
    public string Detail = "";
}

/// <summary>
/// 战斗日志。可序列化/回放（M2 退出标准：战斗日志可回放）。
/// </summary>
public class BattleLog
{
    public List<BattleEvent> Events = new();
    public string Outcome = "";  // player_win / enemy_win / timeout
    public int TotalTicks;

    public void Record(BattleEvent e) => Events.Add(e);
}
