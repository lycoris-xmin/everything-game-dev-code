# 仙侠回合制 RPG 示例

这是一个使用 everything-game-dev-code 脚手架创建的完整 GDD 示例，展示了如何将游戏设计内容填充到 GDD 模板中。

## 项目概述

**仙侠回合制 RPG**（工作标题待定）是一款仙侠题材的 2D 回合制单机 RPG。

### 核心特点
- **核心幻想**: 「这套 Build 是我自己搭的。」
- **游戏支柱**:
  1. 一套 Build 就是你自己的
  2. 每一分努力都被看见
  3. 看懂就能赢
  4. 一个人就能完整体验世界
  5. 仙剑式叙事，但不是仙剑同人

### 技术栈
- **引擎**: Godot 4.7.2 (mono)
- **语言**: C# (.NET 8+)
- **平台**: PC (Windows/Steam)

## 文件结构

```
examples/xianxia-rpg/
├── README.md                    # 本文件
├── game-design-document.md      # 完整 GDD（Rev 2 通过）
├── PLAN.md                      # 里程碑 + 垂直切片计划
├── TDD.md                       # Godot C# 技术设计文档
├── SETUP.md                     # 新手向：安装/打开/运行/导出
├── project.godot               # 工程配置（应用名/分辨率/InputMap/Autoload 注册）
├── icon.svg                     # 占位图标
├── export_presets.cfg           # Windows 导出预设（入版本控制）
├── .gitignore
├── scenes/Main.tscn             # 主场景（M0 启动点）
├── scripts/
│   ├── Main.cs                  # 根节点脚本
│   ├── Autoloads/              # 7 个全局单例占位
│   └── Systems/                # 纯 C# 逻辑层（M1 起填充）
├── resources/                   # Godot 资源（.tres 等）
├── data/                        # 配置表（CSV/JSON，M1 起）
├── tests/                       # 测试（NUnit + Godot 测试运行器）
└── design/gdd/                  # GDD 子系统详细设计（待补 SYS-006 等）
```

## 如何使用

1. **阅读 GDD**: 查看 `game-design-document.md` 了解完整的文档结构
2. **参考填充**: 按照同样的方式填充你自己的 GDD
3. **使用脚手架命令**:
   - `/design-review` - 评审你的 GDD
   - `/design-system` - 设计具体系统
   - `/art-bible` - 建立视觉规范

## 相关资源

- [GDD 模板](../../docs/templates/game-design-document.md)
- [技术设计文档模板](../../docs/templates/technical-design-document.md)
- [GDD 示例 (Emberfall)](../cross-engine-gdd/game-design-document.md)

## 原始项目

本示例基于 [Claude Code Game Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) 项目中的仙侠 RPG 设计内容。

---

*最后更新: 2026-09-16*
