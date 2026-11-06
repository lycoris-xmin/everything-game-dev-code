using XianxiaRPG.Systems.Core;

namespace XianxiaRPG.Systems.Battle;

/// <summary>
/// SYS-007 技能（最小契约）。仅含结算所需字段，演出/资源在 M5 接入。
/// </summary>
public enum SkillKind
{
    Attack,  // 攻击（普攻/仙术）
    Heal,    // 治疗（M2 预留）
    Buff,    // 增益（M2 预留）
    Ultimate // 终结技（神力满释放，可插队）
}

public class Skill
{
    public string Id = "";
    public string Name = "";
    public string Description = "";
    public Element Element;
    public int Power;   // 传给 DamageCalculator 的 skillPower 基数
    public int MpCost;  // 气力消耗
    public SkillKind Kind = SkillKind.Attack;
}
