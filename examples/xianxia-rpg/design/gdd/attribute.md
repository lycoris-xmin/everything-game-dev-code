# SYS-001 / SYS-002 / SYS-004 详细设计 — 属性、五灵与配置（M1）

> 源真文档（Source of Truth）。M1 数据与配置底座的接口与数值以本文为准。
> 关联：`PLAN.md` §2(M1)、`TDD.md` §5.2 / §6.1。
> 注：战斗数值（SYS-006）尚未冻结，本文倍率为 M1 最小契约占位，冻结前可调整。

## 1. 属性系统（SYS-001）

10 项面板数值，**全加法、无乘法项、面板直显**（GDD §5.2 / TDD §5.2）。

| Key | 中文 | 含义 | 影响 |
|-----|------|------|------|
| Attack | 攻击 | 物理基础伤害 | 伤害公式进攻项 |
| Defense | 防御 | 物理减伤 | 伤害公式防守项 |
| MagicPower | 灵力 | 五灵法术攻击 | 伤害公式进攻项（与 Attack 同权加和） |
| MagicResist | 灵抗 | 五灵减伤 | 伤害公式防守项 |
| Speed | 身法 | 行动值速率 | AV 队列推进（M2 使用） |
| CritRate | 暴击率 | 暴击概率(%) | M2 暴击结算 |
| Toughness | 韧性 | 击破抗性 | M2 韧性条 |
| Accuracy | 命中 | 命中率 | M2 命中判定 |
| Evasion | 闪避 | 闪避率 | M2 闪避判定 |
| Luck | 幸运 | 随机偏移/稀有度 | 掉落/暴击微调 |

资源（非属性，由等级/属性派生，单独管理）：精力(HP) / 气力(MP) / 神力(终结技槽)。

**最终值规则**：`Final = Base + Σ Bonus`，纯加法。`AttributeSet.ComputeFinal(key)` 与索引器均返回该值。

## 2. 五灵与元素（SYS-002）

五灵：`Wind 风 / Thunder 雷 / Water 水 / Fire 火 / Earth 土`。

- **相克环**：风→雷→水→火→土→风（攻击方克守方，伤害 ×1.3）
- **相生环**：风→火→雷→土→水→风（攻击方生守方，守方受生减伤 ×0.85）
- 同元素 ×1.0；不克不生 ×1.0
- `ElementRing.GetMultiplier(atk, def)` 为纯函数，无副作用。

## 3. 伤害公式（M1 最小契约，确定性）

```
offense = Attack + MagicPower + SkillPower
defense = Defense + MagicResist
raw     = offense - defense
damage  = max(1, round(raw * ElementRing.GetMultiplier(atkEl, defEl)))
```

- 纯函数、无随机（便于 NUnit 确定性断言）；随机/暴击在 M2 接入。
- `SkillPower` 来自技能配置（M2 接入），M1 测试用固定值。

## 4. 配置表（SYS-004）

- 数值密集型用 CSV（策划友好），位于 `data/`。
- 约定：首行为表头，含 `id` 列；单元格支持引号转义。
- `ConfigLoader`（Autoload）启动时加载 `data/attribute_bases.csv` 到内存字典，
  提供 `GetAttributeBase(id)`。纯解析逻辑在 `Systems/Core/CsvParser`（不依赖 Godot，可测）。

### attribute_bases.csv schema
`id,attack,defense,magic_power,magic_resist,speed,crit_rate,toughness,accuracy,evasion,luck,element`

## 5. 测试（PLAN M1 退出标准）

NUnit（`tests/`）对以下做确定性断言：
1. `AttributeSet` 加法：`Base+Bonus == Final`。
2. `ElementRing`：相克 1.3 / 相生 0.85 / 同元素 1.0 / 无关 1.0。
3. `DamageCalculator`：给定两套属性+元素+SkillPower，伤害值唯一确定。
