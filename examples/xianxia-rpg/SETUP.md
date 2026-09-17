# 新手向：本地搭建与运行（M0）

本目录是一个 **Godot 4.7.2 (Mono) + C# (.NET 8+)** 工程的 M0 脚手架。你（第一次接触）按下面步骤即可跑起来。

## 1. 安装依赖
1. **Godot 4.7.2 (Mono / .NET 版)** —— 必须选带 `Mono` / `.NET` 的版本（不是纯 GDScript 版）。
   - 下载：https://godotengine.org/download/ （选 "Godot 4.7.2 - .NET"）
   - 解压到任意目录，记住 `GodotDotNet.exe` / `Godot` 可执行文件路径
2. **.NET 8 SDK** —— https://dotnet.microsoft.com/download （安装后终端执行 `dotnet --version` 应显示 8.x）
3. （可选）把 Godot 可执行文件加入系统 PATH，方便命令行启动。

## 2. 打开工程
- 双击 `Godot` 可执行文件 → `Import` → 选择本目录的 `project.godot` → `Import & Edit`。
- 首次打开时，Godot 会**自动生成** `*.csproj`、`.mono/` 等 C# 工程文件（已在 `.gitignore` 中忽略，无需提交）。
- 若下方输出面板出现 C# 编译，等待其完成（首次稍慢）。

## 3. 运行
- 按 **F5**（或右上角 ▶ 播放）启动。主场景 `scenes/Main.tscn` 会加载，
  输出面板应打印：`仙侠回合制RPG — M0 工程脚手架就绪`，以及 7 个 `[Autoload] ... 就绪`。
- 看到一个空 2D 视口即代表工程可编译、可运行。

## 4. 导出 Windows 包
- `Project > Export` → 已内置 `Windows Desktop` 预设（`export_presets.cfg`）。
- 首次导出需安装 Windows 导出模板（`Editor > Manage Export Templates > Download`）。
- 导出路径：`export/win/xianxia-rpg.exe`（目录已在 `.gitignore` 忽略）。

## 5. 工程结构（当前）
```
examples/xianxia-rpg/
├── project.godot          # 工程配置（应用名/分辨率/InputMap/Autoload 注册）
├── icon.svg               # 占位图标
├── export_presets.cfg     # Windows 导出预设（入版本控制）
├── .gitignore
├── SETUP.md               # 本文件
├── game-design-document.md# GDD（Rev 2）
├── PLAN.md                # 里程碑 + 垂直切片计划
├── TDD.md                 # Godot C# 技术设计
├── scenes/Main.tscn       # 主场景（M0 启动点）
├── scripts/
│   ├── Main.cs            # 根节点脚本
│   ├── Autoloads/         # 7 个全局单例占位（Save/Progression/Config/Input/EventBus/Telemetry/Platform）
│   └── Systems/           # 待 M1 填充的纯逻辑（战斗/数值/五灵/打造…）
├── resources/             # 资源（.tres 等，待填充）
├── data/                  # 配置表（CSV/JSON，待 M1）
├── tests/                 # 测试（NUnit + Godot 测试运行器，待 M1 定框架）
└── design/                # GDD 子系统详细设计（SYS-006 战斗等，待补）
```

## 6. 常见问题
- **打开后报 C# 找不到 / 编译失败**：确认装的是 `.NET` 版 Godot 且 .NET 8 SDK 已装；重启编辑器让它重新生成 `.csproj`。
- **F5 黑屏**：正常（M0 还没放任何可见内容），看输出面板的打印即可。
- **Autoload 报错 "script not found"**：检查 `project.godot` 中 `[autoload]` 路径与 `scripts/Autoloads/*.cs` 是否一致。
- **想改分辨率/全屏**：`Project > Project Settings > Display > Window`。

## 7. 下一步（实现路线，见 PLAN.md）
M0 完成 → M1 数据与配置（属性/五灵/配置加载）→ M2 回合制战斗 → M3 探索 → M4 掉落/打造 → M5 UI/存档。
