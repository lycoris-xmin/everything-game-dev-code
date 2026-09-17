using System;
using System.Collections.Generic;
using System.Linq;
using XianxiaRPG.Systems.Core;
using XianxiaRPG.Systems.Entities;

namespace XianxiaRPG.Systems.Battle;

/// <summary>
/// SYS-006 战斗数值常量（最小契约，数值冻结前可整体调整）。
/// </summary>
public static class BattleConfig
{
    public const int ActionThreshold = 100;        // AV 行动阈值
    public const int UltimateMax = 100;            // 神力槽上限
    public const int UltimateGainOnAction = 25;    // 每次行动获神力
    public const int UltimateGainOnHit = 10;       // 受击获神力
    public const int ToughnessBreakOnCounter = 25;// 相克攻击削韧性
    public const int ToughnessBreakOnHit = 10;    // 普通攻击削韧性
    public const float BrokenDamageMultiplier = 1.5f; // 击破增伤
    public const int BrokenTurns = 2;             // 击破持续（该单位行动次数）
    public const float DefenseDamageMultiplier = 0.5f; // 防御减伤
    public const int UltimateSkillPower = 50;      // 终结技基础威力
    public const float UltimateMultiplier = 2.0f;  // 终结技倍率
    public const int MaxTicks = 500;              // 防死循环
}

/// <summary>
/// SYS-006 战斗引擎（最小契约）：AV 行动值队列 + 韧性击破 + 可插队终结技。
/// 纯函数式推进、确定性（无随机），产出可回放 BattleLog。
/// </summary>
public static class BattleEngine
{
    public static BattleLog Run(List<Combatant> allies, List<Combatant> enemies, ICombatAI ai)
    {
        var log = new BattleLog();
        var all = allies.Concat(enemies).ToList();
        int tick = 0;

        while (tick < BattleConfig.MaxTicks)
        {
            tick++;
            if (allies.All(a => !a.Alive)) { log.Outcome = "enemy_win"; break; }
            if (enemies.All(e => !e.Alive)) { log.Outcome = "player_win"; break; }

            var actor = PeekNextActor(all);
            if (actor == null) break;

            var foes = (actor.IsEnemy ? allies : enemies).Where(f => f.Alive).ToList();

            Skill skill;
            if (ai.WantsUltimate(actor))
                skill = actor.Skills.First(s => s.Kind == SkillKind.Ultimate);
            else
            {
                skill = ai.ChooseSkill(actor);
                if (actor.Mp < skill.MpCost)
                    skill = actor.Skills.First(s => s.Kind == SkillKind.Attack && s.MpCost == 0);
            }
            var target = ai.ChooseTarget(actor, foes);
            ApplyAction(actor, skill, target, all, log, ref tick);

            actor.Av -= BattleConfig.ActionThreshold;
            actor.Defending = false;

            // 击破恢复：该单位行动后递减，归零则韧性回满
            if (actor.BrokenTurns > 0)
            {
                actor.BrokenTurns--;
                if (actor.BrokenTurns == 0)
                {
                    actor.Toughness = actor.MaxToughness;
                    log.Record(new BattleEvent { Tick = tick, Actor = actor.Name, Action = "recover", Detail = "toughness restored" });
                }
            }
        }

        if (tick >= BattleConfig.MaxTicks && log.Outcome == "") log.Outcome = "timeout";
        log.TotalTicks = tick;
        return log;
    }

    /// <summary>
    /// 选下一行动者：神力满者插队优先；否则推进 AV 至有人达阈值，取 AV 最高者。
    /// 并列按编队顺序确定，保证可复现。
    /// </summary>
    public static Combatant PeekNextActor(List<Combatant> all)
    {
        var alive = all.Where(u => u.Alive).ToList();

        var ready = alive.Where(u => u.Ultimate >= BattleConfig.UltimateMax).ToList();
        if (ready.Count > 0)
            return ready.OrderByDescending(u => u.Ultimate).ThenBy(u => all.IndexOf(u)).First();

        while (alive.All(u => u.Av < BattleConfig.ActionThreshold))
            foreach (var u in alive) u.Av += u.Speed;

        return alive.Where(u => u.Av >= BattleConfig.ActionThreshold)
                    .OrderByDescending(u => u.Av)
                    .ThenBy(u => all.IndexOf(u))
                    .First();
    }

    /// <summary>执行一次行动（供 UI 交互驱动）。封装伤害/削韧/击破/神力/AV 与击破恢复。</summary>
    public static void ApplyAction(Combatant actor, Skill skill, Combatant target, List<Combatant> all, BattleLog log, ref int tick)
    {
        if (skill.Kind == SkillKind.Ultimate)
        {
            int raw = DamageCalculator.ComputeDamage(actor.Attr, actor.Element, target.Attr, target.Element, BattleConfig.UltimateSkillPower);
            int dmg = (int)Math.Round(raw * BattleConfig.UltimateMultiplier);
            ApplyDamage(actor, target, dmg, log, tick);
            actor.Ultimate = 0;
            log.Record(new BattleEvent { Tick = tick, Actor = actor.Name, Action = "ultimate", Target = target.Name, Damage = dmg, Detail = skill.Name });
        }
        else
        {
            if (actor.Mp < skill.MpCost)
                skill = actor.Skills.First(s => s.Kind == SkillKind.Attack && s.MpCost == 0);
            actor.Mp -= skill.MpCost;
            int dmg = DamageCalculator.ComputeDamage(actor.Attr, actor.Element, target.Attr, target.Element, skill.Power);
            if (target.Defending) dmg = (int)Math.Round(dmg * BattleConfig.DefenseDamageMultiplier);
            if (target.BrokenTurns > 0) dmg = (int)Math.Round(dmg * BattleConfig.BrokenDamageMultiplier);
            ApplyDamage(actor, target, dmg, log, tick);
            actor.Ultimate = Math.Min(BattleConfig.UltimateMax, actor.Ultimate + BattleConfig.UltimateGainOnAction);
            log.Record(new BattleEvent { Tick = tick, Actor = actor.Name, Action = "skill", Target = target.Name, Damage = dmg, Detail = skill.Name });
        }
        actor.Av -= BattleConfig.ActionThreshold;
        actor.Defending = false;
        if (actor.BrokenTurns > 0)
        {
            actor.BrokenTurns--;
            if (actor.BrokenTurns == 0)
            {
                actor.Toughness = actor.MaxToughness;
                log.Record(new BattleEvent { Tick = tick, Actor = actor.Name, Action = "recover", Detail = "toughness restored" });
            }
        }
    }

    static void ApplyDamage(Combatant actor, Combatant target, int dmg, BattleLog log, int tick)
    {
        target.Hp -= dmg;

        bool counter = ElementRing.IsCounter(actor.Element, target.Element);
        target.Toughness -= counter ? BattleConfig.ToughnessBreakOnCounter : BattleConfig.ToughnessBreakOnHit;
        if (target.Toughness <= 0 && target.BrokenTurns == 0)
        {
            target.Toughness = 0;
            target.BrokenTurns = BattleConfig.BrokenTurns;
            log.Record(new BattleEvent { Tick = tick, Actor = target.Name, Action = "broken", Detail = "toughness depleted" });
        }

        if (target.Hp <= 0)
        {
            target.Hp = 0;
            log.Record(new BattleEvent { Tick = tick, Actor = target.Name, Action = "death", Detail = "hp depleted" });
        }
        else
        {
            target.Ultimate = Math.Min(BattleConfig.UltimateMax, target.Ultimate + BattleConfig.UltimateGainOnHit);
        }
    }
}
