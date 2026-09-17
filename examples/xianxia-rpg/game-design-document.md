# 仙侠回合制 RPG 游戏设计文档

## Document Control
- **Project**: 仙侠回合制 RPG（工作标题待定）
- **Feature / Product**: 核心玩法验证切片
- **Owner**: Lycoris
- **Contributors**: Claude Code Game Studios 49 Agents
- **Version**: 1.0
- **Status**: Draft (Rev 2 — 2026-09-16 复评通过)
- **Last Updated**: 2026-09-16
- **Related Docs**:
  - `design/gdd/combat-system.md` — 战斗系统详细设计
  - `design/gdd/attribute-system.md` — 属性与数值系统
  - `design/gdd/systems-index.md` — 31个系统索引

---

## 1. Executive Summary

### 1.1 Elevator Pitch
**这是一款仙侠题材的 2D 回合制单机 RPG。你收集材料、亲手打造装备、搭配队伍，走出一条属于自己的路。**

**10 秒测试**：星铁式的回合制战斗 + 从材料亲手打出来的装备养成。

### 1.2 Product Pillars

| 支柱 | 定义 | 设计测试 |
|------|------|----------|
| **① 一套 Build 就是你自己的** | 配队、装备、养成路线的组合，能产出可辨认的个人风格 | 新增一件装备——它是否创造了新的搭配可能？ |
| **② 每一分努力都被看见** | 养成与打造的每一步都必须有可见的反馈 | 一个新系统——它是否提供可见的进度反馈？ |
| **③ 看懂就能赢** | 难度来自决策，不来自信息不透明 | 一个新机制——玩家能否在行动之前预判结果？ |
| **④ 一个人就能完整体验世界** | 主线、支线、战斗、养成全部单人可完成 | 一个设计——如果没有联网就不成立？否决 |
| **⑤ 仙剑式叙事，但不是仙剑同人** | 叙事腔调延续仙剑，但角色、剧情全部原创 | 一段新剧情——没玩过仙剑的人能否看懂并被感动？ |

### 1.3 Player Promise
玩家将体验到：
- **自主性**：配队、打造路径、强化取舍——选择权在你手上
- **能力感**：伤害数字、装备成型、Build 生效——「变强了」肉眼可见
- **连接感**：同伴羁绊、NPC 关系、剧情牵引

---

## 2. Audience and Positioning

### 2.1 Target Audience

| 属性 | 内容 |
|------|------|
| **年龄** | 18–35 |
| **游戏经验** | 中核——熟悉回合制 RPG，享受策略深度但不追求操作难度 |
| **时间** | 工作日 30–60 分钟，周末 2 小时以上 |
| **平台偏好** | PC |
| **目前在玩** | 崩坏：星穹铁道、仙剑奇侠传、梦幻西游 |
| **他要什么** | 「能搭出自己的一套」——有表达空间、有可见成长、有故事可沉浸 |
| **会劝退他的** | P2W、强制组队卡进度、操作复杂、剧情敷衍、无意义的重复刷 |

### 2.2 Market Positioning

| 参考 | 借鉴什么 | 我们怎么做不同 | 为什么重要 |
|------|----------|----------------|------------|
| **崩坏：星穹铁道** | AV 行动值队列、韧性击破、可插队终结技 | 单机、资源不回满 | 验证「可计算」回合制仍有受众 |
| **仙剑奇侠传** | 精气神三资源、独立故事单元 | 原创世界观；系统化程度更高 | 资源纪律带来取舍感 |
| **梦幻西游** | 养成提升的可见反馈 | 去掉 MMO 社交，强化单机沉浸 | 证明回合制循环对长期养成有吸引力 |

---

## 3. Core Experience

### 3.1 Core Fantasy
**「这套 Build 是我自己搭的。」**

从材料到装备到队伍，玩家走的是一条**可辨认的、属于他自己的**成长路径。不是"我抽到了强力角色"，而是"我打出了这把剑、我配出了这套队、我算出了这条路线"。

驱动玩家反复打开游戏的，是**养成提升的可见反馈、装备打造的掌控感、世界的探索、剧情的推进**——战斗是承载这些的舞台，不是目的。

### 3.2 Core Loop

#### 30 秒循环（逐刻）
选择指令 → 技能演出 → 伤害/效果结算 → 轮到下一单位。
操作是点选（普攻 / 仙术 / 防御 / 终结技 / 道具），**复杂度在决策不在操作**。

#### 5–15 分钟循环（短期目标）
一组遭遇（3–5 场战斗）→ 收集掉落材料 → 回到打造台 → 造出/强化一件装备 → 感觉变强。
**「材料差两个」驱动再打一组**，这是"再来一轮"心理的落点。

**支撑规则**：
- 每个敌人 100% 固定掉 1 种标志材料 + 随机 1–2 种通用材料
- MVP 强化成本线性（每级 +50%）
- MVP 强化 100% 成功

#### 一次会话（30–120 分钟）
探索区域 → 遭遇战 → 收集材料与剧情 → 打造与养成 → 推进剧情节点 → 存档退出。

#### 长期成长（天/周）
- **MVP 期**：等级提升 → 装备迭代 → Build 成型 → 挑战更高难区域
- **垂直切片起**：编队取舍 → 经脉 → 命格 → 灵兽 依次补入

### 3.3 Supporting Loops

- **Meta progression loop**: 等级提升 → 装备打造 → Build 成型
- **Session loop**: 探索 → 遭遇 → 收集 → 打造 → 变强
- **Long-term mastery loop**: 搭配优化 → 材料路线规划 → Build 精通

---

## 4. Game Structure

### 4.1 Modes
- **Main gameplay mode**: 回合制战斗 + 区域探索
- **Meta / hub / menu mode**: 打造台、角色面板、存档点
- **Optional modes**: 无（MVP 不含支线、图鉴、成就）

### 4.2 Session Structure
1. **开始**: 从存档点出发
2. **探索**: 在区域中移动，触发遭遇战
3. **战斗**: 回合制战斗，收集材料
4. **打造**: 回到打造台，制造/强化装备
5. **推进**: 解锁剧情节点
6. **结束**: 在存档点存档退出

### 4.3 Progression Structure
- **Unlocks**: 等级提升解锁新技能、新配方
- **Difficulty curve**: 主线战斗温和；难度来自搭配不对而非操作不好
- **Content gating**: 区域按剧情进度解锁
- **Failure / retry structure**: 战斗失败退回最近存档点，不自动回满资源

---

## 5. Systems Overview

### 5.1 属性与数值系统 (SYS-001)
- **Purpose**: 定义玩家面板上的 10 项属性
- **Player goal**: 看懂数字，算清后果
- **Inputs**: 等级、装备、经脉、命格
- **Rules**: 全加法、面板直显、无乘法项
- **Resources**: 精力上限、气力上限、神力上限、外功攻击、内功攻击、外功防御、内功防御、速度、会心率、会心伤害

### 5.2 战斗系统 (SYS-006)
- **Purpose**: AV 队列回合制战斗
- **Player goal**: 用有限资源把每场战斗打得足够省
- **Inputs**: 角色属性、技能、装备、五灵相克
- **Rules**: 行动值队列、韧性击破、可插队终结技
- **Resources**: 精力(HP)、气力(MP)、神力(终结技槽)

### 5.3 装备与打造系统 (SYS-011 + SYS-031)
- **Purpose**: 材料驱动的装备打造与强化
- **Player goal**: 从材料亲手打造出独一无二的装备
- **Inputs**: 掉落材料、配方、金币
- **Rules**: 打造产出各轴独立随机、强化线性成本
- **Resources**: 材料、配方、金币

### 5.4 等级与成长系统 (SYS-003)
- **Purpose**: 角色等级提升
- **Player goal**: 看见角色变强
- **Inputs**: 经验值
- **Rules**: L1-L20，加法成长，总量封顶
- **Resources**: 经验值

### 5.5 五灵与元素系统 (SYS-002)
- **Purpose**: 五灵相克/相生关系
- **Player goal**: 利用属性克制获得优势
- **Inputs**: 角色五灵属性、敌人五灵属性
- **Rules**: 相克环（风→雷→水→火→土→风）、相生环（风→火→雷→土→水→风）

---

## 6. Content Structure

### 6.1 World / Level Structure
- **Level flow**: 线性区域探索
- **Mission / stage structure**: 1 个小型区域、1 段短主线
- **Encounter structure**: 3-5 场战斗为一组遭遇

### 6.2 Content Taxonomy
- **Enemies**: 8–12 种
- **Weapons / abilities**: 15–25 件装备
- **Items**: 材料（每种敌人掉落 1-3 种）
- **Collectibles**: 无（MVP）
- **Rewards**: 经验值、材料、金币
- **NPCs**: 5 名固定角色

### 6.3 Content Scaling Rules
- 等级 L1-L20，每级提升约基础值的 10%
- 装备加成约等于 5-10 级的提升
- 敌人强度随区域线性增长

---

## 7. UX, UI, and Accessibility

### 7.1 UX Goals
- **Clarity goals**: 决策依据同屏可见，不翻菜单
- **Friction to avoid**: 操作复杂、信息不透明
- **Onboarding expectations**: 前 10 分钟完成第一场战斗与第一次打造

### 7.2 UI Surfaces
- **HUD**: AV 队列、韧性条、五灵相克提示、层数徽标
- **Menus**: 角色面板、打造界面、存档点
- **Settings**: 音量、文本缩放、手柄设置
- **Meta screens**: 无（MVP）

### 7.3 Accessibility Goals
- **Input accessibility**: 手柄完整可导航，焦点状态可区分
- **Subtitle / text accessibility**: 文本缩放 100%-200%
- **Visual accessibility**: 状态编码三元冗余（形状+文字+颜色）
- **Cognitive accessibility**: 回合制无时限，可完成全部战斗
- **Audio accessibility**: 击破、暴击、硬控命中有可区分音效

---

## 8. Narrative and Audio

### 8.1 Narrative Role
仙剑式叙事——独立的悲剧与温情、让你记住名字的配角。但角色、剧情、世界观全部原创。

### 8.2 Story Delivery
- **Dialogue**: 对话与剧情
- **Quests**: 任务系统
- **Environmental storytelling**: 场景叙事
- **Cinematics**: 无（MVP 无过场动画）

### 8.3 Audio Direction
- **Music role**: 场景 BGM（古风配乐）
- **SFX role**: 技能音效、UI 音效
- **Voice role**: 无（MVP 无配音）
- **Accessibility implications**: 音频作为无障碍冗余通道

---

## 9. Art and Presentation

### 9.1 Visual Pillars
- **风格**: 2D 手绘古风（俯视 / 等角）
- **MVP 美术**: 占位（色块 / 免费素材 / 临时立绘）

### 9.2 Camera / Framing
- 俯视/等角视角
- 固定镜头

### 9.3 Animation / VFX Role
- 角色动作（骨架动画）
- 技能演出
- 打造与突破的视觉反馈

### 9.4 Readability Rules
- 状态图标编码：形状+文字+颜色三元冗余
- 1080p 与 1440p 下保持可读
- 禁用 1px 线，改用实心形状

---

## 10. Economy and Balance

### 10.1 Resource Model
| 资源 | 战斗内 | 战斗外 |
|------|--------|--------|
| **精力** | 消耗品、恢复仙术 | 消耗品、仙术、客栈、存档点 |
| **气力** | **仅消耗品** | **仅消耗品**、客栈、存档点 |
| **神力** | 战斗内积攒 | 每场战斗从 0 开始 |

### 10.2 Reward Model
- 每个敌人 100% 固定掉 1 种标志材料
- 随机 1–2 种通用材料
- 经验值

### 10.3 Sinks / Costs
- 装备打造消耗材料
- 装备强化消耗材料
- 消耗品消耗气力/金币

### 10.4 Tuning Hypotheses
- 材料差 2 个驱动再打一组
- 单区域材料总量不足以造齐全部装备
- 强化成本线性（每级 +50%）

### 10.5 Abuse / Exploit Risks
- 重复打造逼近最优（刻意接受，不设防刷机制）
- 天然闸门：材料成本本身

---

## 11. Telemetry and Success Metrics

### 11.1 Design Questions
- 战斗 → 材料 → 打造 → 变强 这个循环是否好玩？
- 玩家能否感知到「这是我搭出来的」？

### 11.2 Key Metrics
- **Retention**: 核心假设验证通过率
- **Progression**: 装备打造完成率
- **Difficulty**: 战斗失败率
- **Economy health**: 材料获取与消耗平衡
- **Session quality**: 单次会话时长

### 11.3 Instrumentation Notes
- 追踪装备打造次数
- 追踪材料获取与消耗
- 追踪战斗胜率

---

## 12. Production Constraints

### 12.1 Scope Boundaries
- **MVP**: 1 个小型区域、1 段短主线、8–12 种敌人、15–25 件装备
- **垂直切片**: +1 个区域、真实 2D 手绘资产、编队系统
- **Alpha**: 全主线、全部区域
- **Full Vision**: 完整内容

### 12.2 Dependencies
- Godot 4.7.2 + C# (.NET 8+)
- 2D 动画管线
- 技能演出编排

### 12.3 Risks
- **最高风险**: 范围与产能（3-5 个月 MVP）
- **美术产能**: 单人开发的美术资产体量
- **战斗系统**: 尚未定稿（六轮评审）

### 12.4 Out of Scope
- 实时战斗或动作元素
- 强制联网或社交
- PVP 与竞技排名
- 需要几十小时才展开的核心内容
- 全配音 / AAA 过场

---

## 13. Validation Plan

### 13.1 Prototype Questions
- 核心循环「战斗 → 材料 → 打造」是否成立？
- 玩家能否在 10 分钟内完成第一场战斗与第一次打造？

### 13.2 Playtest Plan
- N = 5–8 名目标画像玩家
- 远程共用同一 build
- 试玩 45–60 分钟
- 问卷验证「这是我搭出来的」

### 13.3 QA Focus Areas
- 手柄导航完整性
- 状态图标三元冗余
- 1080p 与 1440p 布局
- 文本缩放 100%-200%

### 13.4 Release Risks
- 美术产能不足
- 战斗系统复杂度与支柱 3 的张力
- 材料种类与装备件数的算术约束

---

## 14. Open Questions

- **工作标题未定**（模板必需字段，当前为「待定」）
- **打造系统 SYS-031 尚未动笔**
- **编队取舍 SYS-009 在 MVP 中被削减**
- **design/narrative/ 目录不存在**
- **辨识度装置未定稿**
- **单机赛道对标缺失**
- 存档结构：单存档位还是多存档位？

---

## 15. Change Log

| 日期 | 变更 | 负责人 | 原因 |
|------|------|--------|------|
| 2026-09-15 | 首次创建 | Lycoris | 基于《云梦仙途》重写 |
| 2026-09-16 | Rev 1 首次评审 | 8 位领域专家 | 判 NEEDS REVISION，7 项阻塞 |
| 2026-09-16 | Rev 2 复评 | 8 位领域专家 + creative-director | 判 NEEDS REVISION，核心新增：产能算术缺口、数值纪律、风险证伪条件 |
| 2026-09-16 | Rev 2 修订通过 | Lycoris | MVP 重定义为「玩法验证切片」 |

---

## 附录：系统清单

| # | 系统名称 | 类别 | 优先级 | 状态 |
|---|----------|------|--------|------|
| SYS-001 | 属性与数值系统 | Core | MVP | Not Started |
| SYS-002 | 五灵与元素系统 | Core | MVP | Not Started |
| SYS-003 | 等级与成长系统 | Progression | MVP | Not Started |
| SYS-004 | 数据配置系统 | Core | MVP | Not Started |
| SYS-005 | 存档与持久化 | Persistence | MVP | Not Started |
| SYS-006 | 战斗系统 | Gameplay | MVP | In Review |
| SYS-007 | 技能系统 | Gameplay | MVP | Not Started |
| SYS-008 | 角色系统 | Gameplay | MVP | Not Started |
| SYS-009 | 队伍与编成 | Gameplay | VS | Not Started |
| SYS-010 | 敌人与 AI | Gameplay | MVP | Not Started |
| SYS-011 | 装备系统 | Progression | MVP | Not Started |
| SYS-012 | 物品·材料·消耗品 | Economy | MVP | Not Started |
| SYS-013 | 灵兽系统 | Progression | VS | Not Started |
| SYS-014 | 掉落系统 | Economy | MVP | Not Started |
| SYS-015 | 客栈与存档点 | Economy | MVP | Not Started |
| SYS-016 | 世界探索与场景 | Gameplay | MVP | Not Started |
| SYS-017 | 关卡与迷宫 | Gameplay | MVP | Not Started |
| SYS-018 | 任务系统 | Narrative | VS | Not Started |
| SYS-019 | 对话与剧情 | Narrative | MVP | Not Started |
| SYS-020 | 经脉系统 | Progression | VS | Not Started |
| SYS-021 | 命格系统 | Progression | VS | Not Started |
| SYS-022 | 经济与商店 | Economy | VS | Not Started |
| SYS-023 | 战斗 UI | UI | MVP | Not Started |
| SYS-024 | 菜单/角色面板 UI | UI | MVP | Not Started |
| SYS-025 | 世界 UI / HUD | UI | VS | Not Started |
| SYS-026 | 特效 VFX | Presentation | VS | Not Started |
| SYS-027 | 音频系统 | Audio | VS | Not Started |
| SYS-028 | 图鉴与收集 | Meta | Alpha | Not Started |
| SYS-029 | 成就系统 | Meta | Full Vision | Not Started |
| SYS-030 | 无障碍与输入 | Meta | Full Vision | Not Started |
| SYS-031 | 制作与打造系统 | Progression | MVP | Not Started |
