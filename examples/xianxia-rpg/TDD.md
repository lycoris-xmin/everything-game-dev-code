# 仙侠回合制 RPG — 技术设计文档（Godot C#）

## Document Control
- **Project**: 仙侠回合制 RPG（工作标题待定）
- **System / Feature**: 整体工程架构（Godot 4.7.2 Mono + C# .NET 8）
- **Owner**: technical-design-lead
- **Contributors**: architect, gameplay-programmer, godot-reviewer
- **Version**: 1.0
- **Status**: Draft（待审阅确认）
- **Last Updated**: 2026-09-16
- **Related Docs**:
  - `game-design-document.md` — GDD（Rev 2 通过）
  - `PLAN.md` — 里程碑与垂直切片计划
  - `rules/godot/*` — Godot 规范层（project-structure / autoloads / signals / ui-input / build-release / export-platforms / gdscript-csharp）

---

## 1. Summary

### 1.1 Problem Statement
在 Godot 4.7.2 (Mono) + C# (.NET 8+) 上，以单机 PC (Steam) 为目标，构建一套可支撑"星铁式 AV 回合战斗 + 仙剑式叙事 + 材料驱动装备养成"的 2D RPG 工程，且结构在 MVP→VS→Alpha→Full Vision 扩展中保持可维护、可测试、可导出。

### 1.2 Goals
- **Functional**: 战斗(AV队列/五灵/击破)、探索、对话、掉落、打造、等级、存档、UI 均可运行且互相解耦
- **Non-functional**: 2D 稳定 60fps；C# 逻辑可 headless 单元测试；导出 Windows 包可复现；输入/存档/平台集成可替换

### 1.3 Non-Goals
- 不实现实时/动作战斗（GDD §12.4）
- 不实现联网/社交/PVP
- MVP 不含经脉/命格/商店深度/图鉴/成就

---

## 2. Context

### 2.1 Design Intent（来自 GDD）
- 2D 俯视/等角回合制；核心循环 战斗→材料→打造→变强
- 战斗: AV 行动值队列、韧性击破、可插队终结技、五灵相克/相生
- 属性: 10 项，全加法、面板直显、无乘法项
- 资源: 精力(HP)/气力(MP)/神力(终结技槽)；战斗外不回满
- 单机；手柄完整可导航；文本缩放 100%–200%；状态三元冗余

### 2.2 Constraints
- **Engine**: Godot 4.7.2 (Mono)，锁定版本，export 模板需提交
- **Language**: C# (.NET 8+)，纯 C#（不混 GDScript，见 `rules/godot/gdscript-csharp.md`：本作明确单一语言，避免双语接缝）
- **Platform**: PC Windows，Steam 分发；输出 1080p/1440p
- **Tooling**: .NET 8 SDK、Godot Mono、CI（GitHub Actions 或本地）

### 2.3 Assumptions
| 假设 | 错的影响 |
|------|----------|
| 战斗 SYS-006 接口将在 M2 前冻结 | 若延期，UI/数值模块需最小契约兜底（PLAN §3.6） |
| Godot 4.7.2 Mono 与 .NET 8 工具链稳定 | 若 export 异常，M0 即暴露并锁定版本 |
| Steam 仅需成就+云存档初始集成 | 若需 overlay/富 presence，Platform 层预留扩展 |
| 单存档位起步 | 若多存档，SYS-005 需抽象 slot 管理（开放问题） |

---

## 3. System Boundaries

### 3.1 Responsibilities
- **Core/Data**: 属性、五灵、配置加载、存档（引擎无关纯逻辑）
- **Battle**: AV 队列、技能结算、敌人 AI、战斗事件
- **Progression**: 等级、装备、打造、掉落、材料、灵兽（VS）
- **World**: 区域场景、移动、交互、遭遇触发、任务、对话
- **Presentation**: UI(HUD/菜单/打造)、VFX、Audio
- **Platform**: Steam 集成（成就/云存档/overlay）
- **Telemetry**: 事件采集（打造次数/材料流/胜率）

### 3.2 Dependencies
- Battle → Core(属性/五灵/配置)、Progression(装备/技能)、Presentation(UI/演出)
- World → Battle(遭遇)、Narrative(对话/任务)、Save(存档点)
- Platform → Save(云存档)、Telemetry(成就)

### 3.3 External Interfaces
- Godot 场景树 / Resource 序列化
- Steamworks（经 GodotSteam C# 或 Steamworks.NET）
- 文件系统（user:// 存档）

### 3.4 Out of Scope
- 网络对战、实时物理、GDScript 模块

---

## 4. Architecture Overview

### 4.1 High-Level Architecture
分层 + 特性分组（遵循 `skills/godot/godot-project-structure`）：

```
Game (Node2D 根)
 ├─ Autoloads (全局单例, 窄职责)
 │   ├─ SaveService      (SYS-005)
 │   ├─ ProgressionState (等级/装备/材料 运行时态)
 │   ├─ ConfigLoader     (SYS-004 数据配置)
 │   ├─ InputManager     (InputMap 抽象 + 重映射)
 │   ├─ EventBus         (战斗/全局事件总线)
 │   ├─ Telemetry        (事件上报)
 │   └─ PlatformService  (Steam 集成, 可空实现用于离线)
 ├─ Scenes/
 │   ├─ Battle/    (BattleRoot, UnitView, SkillTimeline)
 │   ├─ World/     (Region, PlayerController, Interactable)
 │   ├─ UI/        (HUD, CraftMenu, CharacterPanel, DialogBox)
 │   └─ Meta/      (SavePoint, MainMenu)
 └─ Systems/ (C# 纯逻辑, 不挂节点)
     ├─ Battle/*   (AvQueue, DamageCalculator, FiveElements)
     ├─ Progression/* (CraftSystem, DropTable, LevelCurve)
     └─ Core/*     (Attributes, ElementRing, Rng)
```

### 4.2 Data Flow
- **输入流**: InputMap(action) → InputManager → 当前场景/战斗指令
- **运行态流**: 战斗结算 → 掉落 → ProgressionState(材料+) → 打造 → 属性重算 → UI 刷新
- **持久化流**: SavePoint/退出 → SaveService 序列化 → user://save.sav（可选 Steam 云）
- **事件流**: 战斗/经济关键动作 → EventBus → Telemetry + UI 反馈

### 4.3 Authority / Ownership
- 单机：ProgressionState 为运行时唯一真相源；存档为其持久化镜像
- 战斗内 Entity 状态归属 BattleController，结束即回收

---

## 5. Detailed Design（关键组件）

### 5.1 BattleController（SYS-006 / SYS-007 / SYS-008 / SYS-010）
- **职责**: 维护 AV 行动值队列、推进单位行动、调用 DamageCalculator、派发战斗事件
- **输入**: 参战单位(角色/敌人)、技能表、五灵属性
- **状态**: 单位列表、AV 值、韧性条、神力槽、回合计数
- **接口契约（最小，待 SYS-006 冻结扩展）**:
  - `EnqueueAction(unit, intent)` — intent: Attack/Skill/Defend/Ultimate/Item
  - `ResolveTurn()` — 按 AV 顺序结算，触发五灵克制与击破
  - `OnUnitDefeated`, `OnBattleEnd` 信号
- **失败模式**: 死锁（无可用行动）→ 超时强制防御；日志可回放便于 QA
- **调试钩子**: `BattleLog` 序列化、headless 重放

### 5.2 Attributes & FiveElements（SYS-001 / SYS-002）
- **Attributes**: `AttributeSet` 资源/结构体，10 字段，全加法 `ComputeFinal()`
- **FiveElements**: 静态 `ElementRing` —— 相克(风→雷→水→火→土→风)、相生(风→火→雷→土→水→风)，纯函数 `GetMultiplier(atk, def)`
- **依赖**: 无引擎依赖，可 NUnit 直接测

### 5.3 CraftSystem & DropTable（SYS-011 / SYS-012 / SYS-014 / SYS-031）
- **DropTable**: 每敌 100% 固定 1 标志材料 + 随机 1–2 通用（配置驱动）
- **CraftSystem**: 配方(Resource) → 消耗材料/金币 → 产出装备；各轴独立随机；强化线性成本(每级 +50%)、100% 成功
- **注意**: SYS-031 详细设计未动笔（PLAN §6），本模块接口待其补充后定型

### 5.4 SaveService（SYS-005）
- **职责**: 序列化 ProgressionState + 剧情进度 + 设置到 `user://`
- **格式**: 版本化 JSON 或 Resource `.tres`；建议 JSON（易迁移/可 Steam 云）
- **防作弊**: 本地签名校验（轻量），非强加密
- **开放**: 单/多存档位待定（建议 M5 前定，影响 slot 抽象）

### 5.5 InputManager（遵循 `rules/godot/ui-input.md`）
- **职责**: 集中 InputMap actions（`ui_accept`/`ui_cancel`/`battle_select`/`world_interact`…），意图级输入，支持手柄/键鼠/重映射
- **原则**: 游戏逻辑只响应 action，不硬编码设备

### 5.6 PlatformService（Steam）
- **职责**: 成就解锁、云存档同步、可选 overlay/富 presence
- **实现**: 经 GodotSteam(C#) 或 Steamworks.NET；离线时空实现(INoPlatform)便于开发
- **集成点**: SaveService(云存档)、Telemetry(成就事件)

---

## 6. Data Design

### 6.1 Authored Data
- 配置表: 敌人/技能/装备/材料/配方/区域 → `data/*.tres`(Resource 子类) 或 `data/*.csv`(ConfigLoader 解析)
- 建议: 数值密集型用 CSV/JSON（策划友好），引用型用 Resource

### 6.2 Runtime Data
- `ProgressionState`(内存)、`BattleState`(战斗内临时)

### 6.3 Save Data / Persistence
- `SaveModel { version, progression, storyFlags, settings }` → `user://save_vN.json`
- 云存档: PlatformService 在写盘后推 Steam RemoteStorage

### 6.4 Versioning / Migration
- `version` 字段；读取时按版本补默认，旧档可迁；破坏式变更走迁移函数

---

## 7. Integration Points

### 7.1 UI / UX
- 战斗 HUD 订阅 EventBus(AV/韧性/五灵)；打造/角色面板读 ProgressionState
- 布局遵循 `rules/godot/ui-input.md`：1080p/1440p、文本缩放、焦点可区分

### 7.2 Audio / VFX
- Audio: 程序化/资源音效，击破/暴击/硬控有可区分音效（无障碍冗余）
- VFX: 技能演出编排（VS）

### 7.3 Telemetry
- EventBus → Telemetry：打造次数、材料获取/消耗、战斗胜率（GDD §11.3）

### 7.4 Build / Pipeline
- 导出预设 `export_presets.cfg` 入版本控制；Windows x86_64；CI 可复现

### 7.5 External SDK
- Steamworks（PlatformService 隔离，离线可跑）

---

## 8. Error Handling and Recovery
- 配置缺失: ConfigLoader 启动校验，缺项报明确错误并列出文件
- 存档损坏: 校验失败回退新档，保留损坏档供排查
- 战斗异常: 日志回放 + 强制结算防卡死
- 日志: 分级（Development/Release 门控，遵循 `rules/godot/build-release.md`）

---

## 9. Performance and Memory
### 9.1 Budget
- 2D 场景常驻 < 8MB；单场战斗逻辑帧 < 2ms；稳定 60fps@1080p
### 9.2 Memory
- 资源按需加载/释放；区域切换释放旧 TileSet/Texture
### 9.3 Profiling
- Godot Profiler + NUnit 性能断言（伤害计算批量）
### 9.4 Risks
- 战斗每帧分配（用对象池/值类型避免 GC 尖刺）

---

## 10. Security / Abuse / Compliance
- 存档轻量签名防手改；非强安全（单机）
- Steam 审核: 启动流/输入/存档兼容/性能需达标（`rules/godot/build-release.md`）
- 调试工具/日志在 Release 移除或门控

---

## 11. Testing Strategy
### 11.1 Unit（确定性逻辑）
- 纯 C# 逻辑（Attributes/FiveElements/DamageCalculator/CraftSystem/DropTable）→ NUnit，`dotnet test` headless
- 建议 M0 定框架，M1 起每个系统配断言
### 11.2 Integration
- Godot 测试运行器 或 GUT(C#)：场景启动、战斗完整流程、存档读写
### 11.3 Engine/runtime
- F5 手动 + CI 导出包冒烟
### 11.4 QA Focus
- 手柄导航、文本缩放、状态三元冗余、1080p/1440p
### 11.5 Regression
- 数值/战斗公式变更需回归 NUnit

---

## 12. Rollout and Migration
- 里程碑门控；导出预设评审；存档 `version` 向前兼容；Steam 上线前定范围

---

## 13. Alternatives Considered
- **GDScript vs C#**: 选 C#（用户指定 .NET 8、强类型、可 headless 测试）；`rules/godot/gdscript-csharp.md` 认可
- **配置格式 CSV vs Resource**: 数值用 CSV（策划迭代快），引用用 Resource
- **Steam 集成 GodotSteam vs Steamworks.NET**: 均 C# 可用，PlatformService 隔离便于切换

---

## 14. Risks and Debt
| 风险 | 负责人 | 影响 | 缓解 |
|------|--------|------|------|
| SYS-006 未定稿 | gdd-designer | 高 | M2 最小契约 |
| C#/Mono export | build-engineer | 高 | M0 验证 |
| SYS-031 缺失 | systems-designer | 中 | M4 前补 |
| 单/多存档未定 | technical-design-lead | 中 | M5 前定 |

---

## 15. Open Questions
- 战斗数值冻结条件（阻塞 M2 收口）
- SYS-031 打造详细设计
- 单/多存档位
- C# 测试框架最终选型（NUnit + GUT？）
- Steam 功能范围（成就/云存档/overlay/富 presence）
- 辨识度装置实现方式

---

## 16. Change Log
| 日期 | 变更 | 负责人 | 原因 |
|------|------|--------|------|
| 2026-09-16 | 首次创建 TDD | technical-design-lead | 补齐 Godot C# 技术规划，对齐 GDD/PLAN |
